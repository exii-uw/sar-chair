#include<String.h>
#include "mpr121.h"
#include <Adafruit_MCP3008.h>


// Servo libraries
#include <Servo.h>

// initialize servos
Servo bottomServo;
Servo topServo;
String message = "9,9"; // command from Unity
int angle = 0;

bool useServos = true; // whether the servos should be moved (DEBUG)
int neutralTop = 70; // forward
int maxTop = 130; // on the floor
int minTop = 0; // on the ceiling

int neutralBottom = 130; // forward
int maxBottom = 180; // toward the seat
int minBottom = 0; // behind


#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
#include "Wire.h"
#endif
int i = 0, start = 0, tilt = 0;
Adafruit_MCP3008 adc, adc1;



/*I2C address and interrupt pin for touch sensors on armrest*/
String t = "";
int irqpinC = 2;  // Right arm
byte addrC = 0x5C ;

int irqpinA = 3; // Left arm
byte addrA = 0x5A;
bool readIMU = 0;

int touchStates[12]; //to keep track of the previous touch states

/*FSR_Armrest1 J4*/
#define s17 A0
#define s18 A1
#define s19 A2

/*FSR_Armrest2 J5*/
#define s20 A3
#define s21 A4
#define s22 A5

/*FSR_Head J6*/
#define s23 A6
#define s24 A7
#define s25 A8

//Right and Left Armrest
#define R_Arm A0
#define L_Arm A2

// Output Array containing all sensor values
float output[45];

// ================================================================
// ===                      INITIAL SETUP                       ===
// ================================================================
void setup() {
  Serial1.begin(115200);

  // default starting position for the servos
  if (useServos) {

    output[42] = neutralTop;
    output[43] = neutralBottom;

    topServo.attach(8);
    topServo.write(output[42]);

    bottomServo.attach(7);
    bottomServo.write(output[43]);
  }

  Serial.begin(115200);
  Serial3.begin(9600);

  /* Pin intialization*/
  /*5 Debugging LEDs*/
  pinMode(31, OUTPUT);
  pinMode(33, OUTPUT);
  pinMode(35, OUTPUT);
  pinMode(37, OUTPUT);
  pinMode(39, OUTPUT);

  /*FSR directly connecting to the MCU*/
  pinMode(s17, INPUT);
  pinMode(s18, INPUT);
  pinMode(s19, INPUT);

  pinMode(s20, INPUT);
  pinMode(s21, INPUT);
  pinMode(s22, INPUT);

  pinMode(s23, INPUT);
  pinMode(s24, INPUT);
  pinMode(s25, INPUT);

  pinMode(R_Arm, INPUT);
  pinMode(L_Arm, INPUT);


  /*Interrupt pin for touch sensor*/
  pinMode(irqpinA, INPUT);
  pinMode(irqpinC, INPUT);

  digitalWrite(irqpinA, HIGH); //enable pullup resistor
  digitalWrite(irqpinC, HIGH);
  Wire.begin();

  mpr121_setup(addrA);
  mpr121_setup(addrC);

  //ADC setup to read the FSR on the Back and Butt
  adc.begin(53);
  adc1.begin(49);

  // Write to the Serial 3 to start reading from the IMU
  Serial3.print("1");
}


// ================================================================
// ===                    MAIN PROGRAM LOOP                     ===
// ================================================================
void loop() {
  // read command from Unity
  if (Serial.available() > 0) {
    moveServo();    
  }

  //Read the touch sensor values of right and left arm rest
  readTouchInputs(addrA);
  readTouchInputs(addrC);

  //Reading Upper, Lower back and Butt FSRs
  for (int chan = 0; chan < 8; chan++) {
    output[chan] = adc.readADC(chan);
  }

  for (int chan = 0; chan < 8; chan++) {
    output[chan + 8] = adc1.readADC(chan);
  }

  // FSR of the right and Left arm
  output[40] = analogRead(R_Arm);
  output[41] = analogRead(L_Arm);

  // Read servo data
  output[42] = topServo.read();
  output[43] = bottomServo.read();



  if (Serial1.available() > 0)
  {
    String val = Serial1.readStringUntil('\n');
    if (val.length() <= 4) {
      output[44] = val.toInt();
    }
  }
  outputFunction(); // The output speed of IMU data is three times slower than the other one. Therefore only send out the data when IMU is ready
  delay(1000/30);
}

void defaultHandler() {
  Serial.println("Invalid input.");
}

/** Servo Functions **/
void moveServo() {
  message = Serial.readStringUntil('\n');
  int index = message.indexOf(',') + 1;
  angle = message.substring(index).toInt();
 
  if (message.indexOf("2,") >= 0) {
    if (angle > maxBottom) {
      angle = maxBottom;
    }

    else if (angle < minBottom) {
      angle = minBottom;
    }

    output[43] = angle;
  }

  // the top servo angle needs to adjust based on the bottom boundaries
  if (message.indexOf("1,") >= 0) {
    if (angle > maxTop) {
      angle = maxTop;
    }

    else if (angle < minTop) {
      angle = minTop;
    }

//    output[42] = angle;
//  }

    // taking the bottom servo angle into account
    if (output[43] >= 170) { // inner side
      if (angle >= 100) {
        angle = 100;
      }
    }

    else if (output[43] <= 30) {

      // prevent from hitting the camera
      if (angle >= 100) {
        angle = 100;
      }

      // prevent from hitting the inner mount wood
      else if (angle <= 40) {
        angle = 40;
      }
    }
    output[42] = angle;
  }

  if (useServos) {
    bottomServo.write(output[43]);
    //delay(20);
    topServo.write(output[42]);
    //delay(20);
  }


}

void outputFunction() {
  for (int i = 0; i < 45; i++) {
    if (useServos && i == 43){
      output[43] = bottomServo.read();
    }

    else if (useServos && i == 42){
      output[42] = topServo.read();
    }
    Serial.print(output[i]);
    Serial.print(",");
  }
//  Serial.print(topServo.read());
//  Serial.print(',');
//  Serial.print(bottomServo.read());
//  Serial.print(',');
//  Serial.print(message);
  Serial.println();
}


// ================================================================
// ===          CUSTOM FUNCTION FOR TOUCH SENSORS               ===
// ================================================================
void readTouchInputs(byte addr) {
  if (!checkInterrupt(addr)) {
    //Read the touch state from the MPR121
    Wire.requestFrom(addr, 2);

    byte LSB = Wire.read();
    byte MSB = Wire.read();

    uint16_t touched = ((MSB << 8) | LSB); //16bits that make up the touch states

    for (int i = 0; i < 12; i++) { // Check what electrodes were pressed
      if (touched & (1 << i)) {
        if (touchStates[i] == 0) {
          //pin i was just touched
          //Serial.print("pin ");
          //Serial.print(i);
          // Serial.println(" was just touched");

        } else if (touchStates[i] == 1) {
          //pin i is still being touched
        }

        touchStates[i] = 1;
      } else {
        if (touchStates[i] == 1) {
          // Serial.print("pin ");
          // Serial.print(i);
          //Serial.println(" is no longer being touched");

          //pin i is no longer being touched
        }
        touchStates[i] = 0;
      }
    }

    if (addr == 0x5A) { // Armrest 1
      for (int i = 0; i < 12; i++) {
        output[i + 16] = touchStates[i];
      }
    }
    else if (addr == 0x5C) { // Armrest 2
      for (int i = 0; i < 12; i++) {
        output[i + 28] = touchStates[i];
        //Serial.print(touchStates[i]);
      }
      // Serial.println("");

    }
  }
}

void mpr121_setup(byte addr) {
  set_register(addr, ELE_CFG, 0x00);

  // Section A - Controls filtering when data is > baseline.
  set_register(addr, MHD_R, 0x01);
  set_register(addr, NHD_R, 0x01);
  set_register(addr, NCL_R, 0x00);
  set_register(addr, FDL_R, 0x00);

  // Section B - Controls filtering when data is < baseline.
  set_register(addr, MHD_F, 0x01);
  set_register(addr, NHD_F, 0x01);
  set_register(addr, NCL_F, 0xFF);
  set_register(addr, FDL_F, 0x02);

  // Section C - Sets touch and release thresholds for each electrode
  set_register(addr, ELE0_T, TOU_THRESH);
  set_register(addr, ELE0_R, REL_THRESH);

  set_register(addr, ELE1_T, TOU_THRESH);
  set_register(addr, ELE1_R, REL_THRESH);

  set_register(addr, ELE2_T, TOU_THRESH);
  set_register(addr, ELE2_R, REL_THRESH);

  set_register(addr, ELE3_T, TOU_THRESH);
  set_register(addr, ELE3_R, REL_THRESH);

  set_register(addr, ELE4_T, TOU_THRESH);
  set_register(addr, ELE4_R, REL_THRESH);

  set_register(addr, ELE5_T, TOU_THRESH);
  set_register(addr, ELE5_R, REL_THRESH);

  set_register(addr, ELE6_T, TOU_THRESH);
  set_register(addr, ELE6_R, REL_THRESH);

  set_register(addr, ELE7_T, TOU_THRESH);
  set_register(addr, ELE7_R, REL_THRESH);

  set_register(addr, ELE8_T, TOU_THRESH);
  set_register(addr, ELE8_R, REL_THRESH);

  set_register(addr, ELE9_T, TOU_THRESH);
  set_register(addr, ELE9_R, REL_THRESH);

  set_register(addr, ELE10_T, TOU_THRESH);
  set_register(addr, ELE10_R, REL_THRESH);
  //0x06 0x0A
  set_register(addr, ELE11_T, TOU_THRESH);
  set_register(addr, ELE11_R, REL_THRESH);

  // Section D
  // Set the Filter Configuration
  // Set ESI2
  set_register(addr, FIL_CFG, 0x04);

  // Section E
  // Electrode Configuration
  // Set ELE_CFG to 0x00 to return to standby mode
  set_register(addr, ELE_CFG, 0x0C);  // Enables all 12 Electrodes
}

boolean checkInterrupt(byte addr) {
  if (addr == 0x5C) {
    return digitalRead(irqpinC);
  }
  else {
    return digitalRead(irqpinA);
  }
}

void set_register(int address, unsigned char r, unsigned char v) {
  Wire.beginTransmission(address);
  Wire.write(r);
  Wire.write(v);
  Wire.endTransmission();
}
