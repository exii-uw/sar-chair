#include "mpr121.h"
#include <I2Cdev.h>
#include <MPU6050_6Axis_MotionApps20.h>

// Servo libraries
#include <Servo.h>
#include <SoftwareSerial.h>
#include <SerialCommand.h>


#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
#include "Wire.h"
#endif

// initialize servos
Servo bottomServo;
Servo topServo;

// helper to communicate with Unity
SerialCommand sCmd;

/*I2C address for IMU*/
// class default I2C address is 0x68
// specific I2C addresses may be passed as a parameter here
// AD0 low = 0x68 (default for SparkFun breakout and InvenSense evaluation board)
// AD0 high = 0x69
MPU6050 mpu;
//MPU6050 mpu(0x69); // <-- use for AD0 high

// uncomment "OUTPUT_READABLE_QUATERNION" if you want to see the actual
// quaternion components in a [w, x, y, z] format (not best for parsing
// on a remote host such as Processing or something though)
//#define OUTPUT_READABLE_QUATERNION

// uncomment "OUTPUT_READABLE_EULER" if you want to see Euler angles
// (in degrees) calculated from the quaternions coming from the FIFO.
// Note that Euler angles suffer from gimbal lock (for more info, see
// http://en.wikipedia.org/wiki/Gimbal_lock)
//#define OUTPUT_READABLE_EULER

// uncomment "OUTPUT_READABLE_YAWPITCHROLL" if you want to see the yaw/
// pitch/roll angles (in degrees) calculated from the quaternions coming
// from the FIFO. Note this also requires gravity vector calculations.
// Also note that yaw/pitch/roll angles suffer from gimbal lock (for
// more info, see: http://en.wikipedia.org/wiki/Gimbal_lock)
#define OUTPUT_READABLE_YAWPITCHROLL

// uncomment "OUTPUT_READABLE_REALACCEL" if you want to see acceleration
// components with gravity removed. This acceleration reference frame is
// not compensated for orientation, so +X is always +X according to the
// sensor, just without the effects of gravity. If you want acceleration
// compensated for orientation, us OUTPUT_READABLE_WORLDACCEL instead.
//#define OUTPUT_READABLE_REALACCEL

// uncomment "OUTPUT_READABLE_WORLDACCEL" if you want to see acceleration
// components with gravity removed and adjusted for the world frame of
// reference (yaw is relative to initial orientation, since no magnetometer
// is present in this case). Could be quite handy in some cases.
//#define OUTPUT_READABLE_WORLDACCEL

// uncomment "OUTPUT_TEAPOT" if you want output that matches the
// format used for the InvenSense teapot demo
//#define OUTPUT_TEAPOT

#define INTERRUPT_PIN 19  // use pin 2 on Arduino Uno & most boards
#define LED_PIN 13 // (Arduino is 13, Teensy is 11, Teensy++ is 6)
bool blinkState = false;

// MPU control/status vars
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer

// orientation/motion vars
Quaternion q;           // [w, x, y, z]         quaternion container
VectorInt16 aa;         // [x, y, z]            accel sensor measurements
VectorInt16 aaReal;     // [x, y, z]            gravity-free accel sensor measurements
VectorInt16 aaWorld;    // [x, y, z]            world-frame accel sensor measurements
VectorFloat gravity;    // [x, y, z]            gravity vector
float euler[3];         // [psi, theta, phi]    Euler angle container
float ypr[3];           // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector

// packet structure for InvenSense teapot demo
uint8_t teapotPacket[14] = { '$', 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0x00, 0x00, '\r', '\n' };

/*I2C address and interrupt pin for touch sensors on armrest*/
String t = "";
int irqpinC = 2;  // Left arm
byte addrC = 0x5C ;

int irqpinA = 3; // Right arm
byte addrA = 0x5A;

boolean touchStates[12]; //to keep track of the previous touch states

/*Select pins on MUX*/
#define s0 22
#define s1 24
#define s2 26
#define s3 28
/*

  /*FSR_Armrest1 J4*/
#define s17 A1
#define s18 A2
#define s19 A3

/*FSR_Armrest2 J5*/
#define s20 A4
#define s21 A5
#define s22 A6

/*FSR_Head J6*/
#define s23 A7
#define s24 A8
#define s25 A9

/*Truth table - select index number in a4, a3, a2 and a1
  FSR_Upperback J3
  1,2,3,4,15

  FSR_Lowerback J7
  5,6,7,8,9

  FSR_Butt J1
  10,11,12,13,14

  No more Height J2 and S5 is not used
*/

int a4[16] = {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1};
int a3[16] = {0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1};
int a2[16] = {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1};
int a1[16] = {0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1};

// MUX Analog output value
int muxAnalog = 0;

// Output Array containing all sensor values
float output[56];


// ================================================================
// ===        INTERRUPT DETECTION ROUTINE FOR IMU               ===
// ================================================================
volatile bool mpuInterrupt = false;     // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
  mpuInterrupt = true;
}


// ================================================================
// ===                      INITIAL SETUP                       ===
// ================================================================
void setup() {
  // servo command
  sCmd.addDefaultHandler(defaultHandler);
  sCmd.addCommand("MOVE", moveServo); // command to move a single servo
 
  bottomServo.attach(7);
  topServo.attach(8);
  bottomServo.write(90);
  topServo.write(90);

  /* Pin intialization*/
  /*5 Debugging LEDs*/
  pinMode(31, OUTPUT);
  pinMode(33, OUTPUT);
  pinMode(35, OUTPUT);
  pinMode(37, OUTPUT);
  pinMode(39, OUTPUT);

  /*select pin on MUX*/
  pinMode(s0, OUTPUT);
  pinMode(s1, OUTPUT);
  pinMode(s2, OUTPUT);
  pinMode(s3, OUTPUT);

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

  // To keep the output value index same as the select pin. No s0 and s16
  output[0] = 0;
  output[16] = 0;

  /*Interrupt pin for touch sensor*/
  pinMode(irqpinA, INPUT);
  pinMode(irqpinC, INPUT);


  digitalWrite(irqpinA, HIGH); //enable pullup resistor
  digitalWrite(irqpinC, HIGH);
  Wire.begin();

  mpr121_setup(addrA);
  mpr121_setup(addrC);

  /*Setup for IMU*/
  // join I2C bus (I2Cdev library doesn't do this automatically)
#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
  Wire.begin();
  Wire.setClock(400000); // 400kHz I2C clock. Comment this line if having compilation difficulties
#elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
  Fastwire::setup(400, true);
#endif

  // initialize serial communication
  // (115200 chosen because it is required for Teapot Demo output, but it's
  // really up to you depending on your project)
  Serial.begin(115200);
  while (!Serial); // wait for Leonardo enumeration, others continue immediately

  // NOTE: 8MHz or slower host processors, like the Teensy @ 3.3V or Arduino
  // Pro Mini running at 3.3V, cannot handle this baud rate reliably due to
  // the baud timing being too misaligned with processor ticks. You must use
  // 38400 or slower in these cases, or use some kind of external separate
  // crystal solution for the UART timer.

  // initialize device
  //Serial.println(F("Initializing I2C devices..."));
  mpu.initialize();
  pinMode(INTERRUPT_PIN, INPUT);

  // verify connection
  //Serial.println(F("Testing device connections..."));
  Serial.println(mpu.testConnection() ? F("MPU6050 connection successful") : F("MPU6050 connection failed"));

  /*// wait for ready
    Serial.println(F("\nSend any character to begin DMP programming and demo: "));
    while (Serial.available() && Serial.read()); // empty buffer
    while (!Serial.available());                 // wait for data
    while (Serial.available() && Serial.read()); // empty buffer again*/

  // load and configure the DMP
  Serial.println(F("Initializing DMP..."));
  devStatus = mpu.dmpInitialize();

  // supply your own gyro offsets here, scaled for min sensitivity
  mpu.setXGyroOffset(220);
  mpu.setYGyroOffset(76);
  mpu.setZGyroOffset(-85);
  mpu.setZAccelOffset(1788); // 1688 factory default for my test chip

  // make sure it worked (returns 0 if so)
  if (devStatus == 0) {
    // turn on the DMP, now that it's ready
    Serial.println(F("Enabling DMP..."));
    mpu.setDMPEnabled(true);

    // enable Arduino interrupt detection
    Serial.print(F("Enabling interrupt detection (Arduino external interrupt "));
    Serial.print(digitalPinToInterrupt(INTERRUPT_PIN));
    Serial.println(F(")..."));
    attachInterrupt(digitalPinToInterrupt(INTERRUPT_PIN), dmpDataReady, RISING);
    mpuIntStatus = mpu.getIntStatus();

    // set our DMP Ready flag so the main loop() function knows it's okay to use it
    Serial.println(F("DMP ready! Waiting for first interrupt..."));
    dmpReady = true;

    // get expected DMP packet size for later comparison
    packetSize = mpu.dmpGetFIFOPacketSize();
  } else {
    // ERROR!
    // 1 = initial memory load failed
    // 2 = DMP configuration updates failed
    // (if it's going to break, usually the code will be 1)
    Serial.print(F("DMP Initialization failed (code "));
    Serial.print(devStatus);
    Serial.println(F(")"));
  }

  // configure LED for output
  pinMode(LED_PIN, OUTPUT);
}

// ================================================================
// ===                    MAIN PROGRAM LOOP                     ===
// ================================================================
void loop() {
  if (Serial.available() > 0) {
    sCmd.readSerial();
  }

  //  // if programming failed, don't try to do anything
  //    if (!dmpReady) return;
  //
  //    // wait for MPU interrupt or extra packet(s) available
  //    while (!mpuInterrupt && fifoCount < packetSize) {
  //        if (mpuInterrupt && fifoCount < packetSize) {
  //          // try to get out of the infinite loop
  //          fifoCount = mpu.getFIFOCount();
  //        }
  //                // other program behavior stuff here
  //
  //        // if you are really paranoid you can frequently test in between other
  //        // stuff to see if mpuInterrupt is true, and if so, "break;" from the
  //        // while() loop to immediately process the MPU data
  //        // .
  //        // .
  //        // .

  readTouchInputs(addrA);
  readTouchInputs(addrC);

  //    // reset interrupt flag and get INT_STATUS byte
  //    mpuInterrupt = false;
  //    mpuIntStatus = mpu.getIntStatus();
  //    // get current FIFO count
  //    fifoCount = mpu.getFIFOCount();
  //
  //    // check for overflow (this should never happen unless our code is too inefficient)
  //    if ((mpuIntStatus & _BV(MPU6050_INTERRUPT_FIFO_OFLOW_BIT)) || fifoCount >= 1024) {
  //        // reset so we can continue cleanly
  //        mpu.resetFIFO();
  //        fifoCount = mpu.getFIFOCount();
  //        //Serial.println(F("FIFO overflow!"));
  //
  //    // otherwise, check for DMP data ready interrupt (this should happen frequently)
  //    } else if (mpuIntStatus & _BV(MPU6050_INTERRUPT_DMP_INT_BIT)) {
  //        // wait for correct available data length, should be a VERY short wait
  //        while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();
  //
  //        // read a packet from FIFO
  //        mpu.getFIFOBytes(fifoBuffer, packetSize);
  //
  //        // track FIFO count here in case there is > 1 packet available
  //        // (this lets us immediately read more without waiting for an interrupt)
  //        fifoCount -= packetSize;
  //
  //        #ifdef OUTPUT_READABLE_YAWPITCHROLL
  //            // display Euler angles in degrees
  //            mpu.dmpGetQuaternion(&q, fifoBuffer);
  //            mpu.dmpGetGravity(&gravity, &q);
  //            mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
  //            Serial.print("Yaw\t");
  //            Serial.print(ypr[0] * 180/M_PI);
  //            output[51] = ypr[0] * 180/M_PI;
  //            Serial.println(output[51]);
  //            Serial.print("\t");
  //            Serial.print(ypr[1] * 180/M_PI);
  //            output[52] = ypr[1] * 180/M_PI;
  //            Serial.print("\t");
  //            Serial.println(ypr[2] * 180/M_PI);
  //            output[53] = ypr[2] * 180/M_PI;
  //        #endif
  //
  //    }

  /***********/

  /* Interate through 1 - 15 to read all FSR sensors values using MUX
    Upper back, Lower back and Butt */
  for (int i = 1; i < 16; i++) {
    digitalWrite(s3, a4[i]);
    digitalWrite(s2, a3[i]);
    digitalWrite(s1, a2[i]);
    digitalWrite(s0, a1[i]);
    delay(10);

    muxAnalog = analogRead(A0);
    output[i] = muxAnalog;
  }

  /* Interate through to read all FSR sensors values Armrest 1,2 and Head*/
  output[17] = analogRead(s17);
  output[18] = analogRead(s18);
  output[19] = analogRead(s19);

  output[20] = analogRead(s20);
  output[21] = analogRead(s21);
  output[22] = analogRead(s22);

  output[23] = analogRead(s23);
  output[24] = analogRead(s24);
  output[25] = analogRead(s25);

  output[54] = getTopAngle();
  output[55] = getBottomAngle();
  
  //debugPrint();
  printOutput();

}

void printOutput() {
  for (int i = 0; i < 56; i++) {
    if(i == 0 || i == 16){
      continue;
    }
    Serial.print(output[i]);
    if (i < 55) {
      Serial.print(",");
    }
  }
  Serial.println();
}
void debugPrint() {
  /*Serial printout formatting*/
  Serial.print("Upper Back ");
  for (int j = 1; j < 5; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[15]);

  Serial.print("Lower Back ");
  for (int j = 5; j < 9; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[9]);

  Serial.print("Butt ");
  for (int j = 10; j < 14; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[14]);

  Serial.print("Armrest right FSR ");
  for (int j = 17; j < 19; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[19]);

  Serial.print("Armrest left FSR ");
  for (int j = 20; j < 22; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[22]);

  Serial.print("Head ");
  for (int j = 23; j < 25; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[25]);

  Serial.print("Arm rest1 Touch ");
  for (int j = 26; j < 37; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[37]);

  Serial.print("Arm rest2 Touch ");
  for (int j = 39; j < 50; j ++) {
    Serial.print(output[j]); Serial.print(" ");
  }
  Serial.println(output[50]);

  Serial.print("Servos ");
  Serial.print(output[54]);
  Serial.print(" ");
  Serial.println(output[55]);

  delay(2);
}

void defaultHandler() {
  Serial.println("Invalid input.");
}

/** Servo Functions **/
// moving a servo (MOVE [servo number] [servo angle])
void moveServo() {
  char *arg;
  int servoNum;
  int angle;

  // getting the servoNum
  arg = sCmd.next();
  if (arg != NULL) {
    servoNum = atoi(arg);
  } else {
    Serial.println("No Servo specified.");
  }

  // getting the angle for the servo
  arg = sCmd.next();
  if (arg != NULL) {
    angle = atof(arg);
    if (servoNum == 1) {
      if (angle != bottomServo.read()) {
        bottomServo.write(angle);
      }
    } else {
      if (angle != topServo.read()) {
        topServo.write(angle);
      }
    }
  } else {
    Serial.println("No angle given for Servo 2");
  }
}

float getTopAngle() {
  return (topServo.read());
}

float getBottomAngle() {
  return (bottomServo.read());
}

// sweep servos
void servoSweep() {
  for (int i = 0; i < 180; i++) {
    bottomServo.write(i);
    topServo.write(i);
    delay(15);
  }

  for (int i = 180; i > -1; i--) {
    bottomServo.write(i);
    topServo.write(i);
    delay(15);
  }
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
          //Serial.println(" was just touched");

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
  }

  if (addr == 0x5A) { // Armrest 1
    for (int i = 0; i < 12; i++) {
      output[i + 26] = touchStates[i];
    }
  }
  else if (addr == 0x5C) { // Armrest 2
    for (int i = 0; i < 12; i++) {
      output[i + 39] = touchStates[i];
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
  set_register(addr, ELE0_T, 0x06);
  set_register(addr, ELE0_R, 0x04);

  set_register(addr, ELE1_T, TOU_THRESH);
  set_register(addr, ELE1_R, REL_THRESH);

  set_register(addr, ELE2_T, TOU_THRESH);
  set_register(addr, ELE2_R, REL_THRESH);

  set_register(addr, ELE3_T, 0x30);
  set_register(addr, ELE3_R, 0x25);

  set_register(addr, ELE4_T, 0x20);
  set_register(addr, ELE4_R, 0x15);

  set_register(addr, ELE5_T, TOU_THRESH);
  set_register(addr, ELE5_R, REL_THRESH);

  set_register(addr, ELE6_T, 0x20);
  set_register(addr, ELE6_R, 0x15);

  set_register(addr, ELE7_T, 0x35);
  set_register(addr, ELE7_R, 0x34);

  set_register(addr, ELE8_T, 0x35);
  set_register(addr, ELE8_R, 0x32);

  set_register(addr, ELE9_T, 0x35);
  set_register(addr, ELE9_R, 0x32);

  set_register(addr, ELE10_T, 0x40);
  set_register(addr, ELE10_R, 0x40);
  //0x06 0x0A
  set_register(addr, ELE11_T, 0x30);
  set_register(addr, ELE11_R, 0x01);

  // Section D
  // Set the Filter Configuration
  // Set ESI2
  set_register(addr, FIL_CFG, 0x04);

  // Section E
  // Electrode Configuration
  // Set ELE_CFG to 0x00 to return to standby mode
  set_register(addr, ELE_CFG, 0x0C);  // Enables all 12 Electrodes

  // Section F
  // Enable Auto Config and auto Reconfig
  /*set_register(addr, ATO_CFG0, 0x0B);
    set_register(addr, ATO_CFGU, 0xC9);  // USL = (Vdd-0.7)/vdd*256 = 0xC9 @3.3V   set_register(addr, ATO_CFGL, 0x82);  // LSL = 0.65*USL = 0x82 @3.3V
    set_register(addr, ATO_CFGT, 0xB5);*/  // Target = 0.9*USL = 0xB5 @3.3V

  set_register(addr, ELE_CFG, 0x0C);
}

boolean checkInterrupt(int interpin) {
  return digitalRead(interpin);
}

void set_register(int address, unsigned char r, unsigned char v) {
  Wire.beginTransmission(address);
  Wire.write(r);
  Wire.write(v);
  Wire.endTransmission();
}
