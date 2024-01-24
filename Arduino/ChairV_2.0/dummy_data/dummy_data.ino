#include <Servo.h>

Servo topServo;  // create servo object to control a servo
Servo bottomServo;
bool useServos = false; // whether the servos should be moved (DEBUG)

int output[45];
String message = "\0";
int angle = 0;

int neutralTop = 70; // forward
int maxTop = 130; // on the floor
int minTop = 0; // on the ceiling

int neutralBottom = 130; // forward
int maxBottom = 180; // toward the seat
int minBottom = 0; // behind

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
  for (int i = 0; i < 45; i++) {
    output[i] = i;
  }

  // default starting position for the servos
  if (useServos) {

    output[42] = neutralTop;
    output[43] = neutralBottom;

    topServo.attach(22);
    topServo.write(output[42]);

    bottomServo.attach(23);
    bottomServo.write(output[43]);
  }
}

void loop() {
  // reading in 'servo data'
  while (Serial.available() > 0) {
    message = Serial.readString();
    int index = message.indexOf(",") + 1;
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

      // taking the bottom servo angle into account
      if (output[43] >= 170){ // inner side
        if (angle >= 100) {
          angle = 100;
        }
      }

      else if (output[43] <= 30){

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



  }

    if (useServos) {
      bottomServo.write(output[43]);
      topServo.write(output[42]);
    }


// put your main code here, to run repeatedly
//  Serial.print(message);
//  Serial.print('_');
//  Serial.print(count);
//  Serial.print(',');
for (int i = 0; i < 45; i++) {
  Serial.print(output[i]);
  Serial.print(",");
}
Serial.println();
Serial.flush();

// delay
delay(1000 / 30);
}
