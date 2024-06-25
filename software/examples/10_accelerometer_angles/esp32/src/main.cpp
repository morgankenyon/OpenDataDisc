/***
 * Morgan Kenyon
 * example 10, measuring angles
 * https://www.youtube.com/watch?v=7VW_XVbtu9k
 */

#include <Wire.h>
#include <Arduino.h>
#include <Adafruit_MPU6050.h>

Adafruit_MPU6050 mpu;

//gyro values
float RateRoll, RatePitch, RateYaw;

//accelerometer values
float AccX, AccY, AccZ;

//calibration values
float CalibX = -0.04;
float CalibY = -0.01;
float CalibZ = + 0.26;

//output values
float AngleRoll, AnglePitch;

//prints out how the mpu sensor is setup
void printAccelerometerStats()
{
  Serial.print("Accelerometer range set to: ");
  switch (mpu.getAccelerometerRange()) {
    case MPU6050_RANGE_2_G:
      Serial.println("+-2G");
      break;
    case MPU6050_RANGE_4_G:
      Serial.println("+-4G");
      break;
    case MPU6050_RANGE_8_G:
      Serial.println("+-8G");
      break;
    case MPU6050_RANGE_16_G:
      Serial.println("+-16G");
      break;
  }

  Serial.print("Gyro range set to: ");
  switch (mpu.getGyroRange()) {
    case MPU6050_RANGE_250_DEG:
      Serial.println("+- 250 deg/s");
      break;
    case MPU6050_RANGE_500_DEG:
      Serial.println("+- 500 deg/s");
      break;
    case MPU6050_RANGE_1000_DEG:
      Serial.println("+- 1000 deg/s");
      break;
    case MPU6050_RANGE_2000_DEG:
      Serial.println("+- 2000 deg/s");
      break;
  }

  Serial.print("Filter bandwidth set to: ");
  switch (mpu.getFilterBandwidth()) {
    case MPU6050_BAND_260_HZ:
      Serial.println("260 Hz");
      break;
    case MPU6050_BAND_184_HZ:
      Serial.println("184 Hz");
      break;
    case MPU6050_BAND_94_HZ:
      Serial.println("94 Hz");
      break;
    case MPU6050_BAND_44_HZ:
      Serial.println("44 Hz");
      break;
    case MPU6050_BAND_21_HZ:
      Serial.println("21 Hz");
      break;
    case MPU6050_BAND_10_HZ:
      Serial.println("10 Hz");
      break;
    case MPU6050_BAND_5_HZ:
      Serial.println("5 Hz");
      break;
  }
}
void setup() {
  // Start serial communication 
  Serial.begin(115200);
  Wire.begin(6, 5);

  // Init BME Sensor
  if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050 chip");
    while (1) {
      delay(10);
    }
  }
  Serial.println("MPU6050 Found!");

  //accelerometer setup
  mpu.setAccelerometerRange(MPU6050_RANGE_8_G);
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_10_HZ);
  printAccelerometerStats();
}

void loop() {
  
  //read accelerometer data
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  /* Print out the values */
  AccX = (float)a.acceleration.x/9.81 + CalibX;
  AccY = (float)a.acceleration.y/9.81 + CalibY;
  AccZ = (float)a.acceleration.z/9.81 + CalibZ;

  RateRoll = (float)g.gyro.x/65.6;
  RatePitch = (float)g.gyro.y/65.5;
  RateYaw = (float)g.gyro.z/65.5;
  AngleRoll = atan(AccY/sqrt(AccX*AccX+AccZ*AccZ))*1/(3.142/180);
  AnglePitch = atan(AccX/sqrt(AccY*AccY+AccZ*AccZ))*1/(3.142/180);

  Serial.print("Roll angle [*]= ");
  Serial.print(AngleRoll);
  Serial.print(", Pitch angle [*]= ");
  Serial.print(AnglePitch);
  Serial.println("");
  delay(1500);
}
