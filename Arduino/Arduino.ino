void setup() {
  Serial.begin(9600);      // Initialize the serial port and set the baud rate to 9600
  pinMode(10, INPUT_PULLUP); // Primary Button 
  pinMode(7, INPUT_PULLUP); // Secondary Button
  pinMode(4, INPUT_PULLUP); // Joystick Button
}

void loop() {
  // 'Primary Button','Secondary Button','Joystick Button','Joystick X','Joystick Y','Wheel Angle','Clutch','Break','Accelerator'
  Serial.print(digitalRead(10)); // Primary Button
  Serial.print(",");
  Serial.print(digitalRead(7)); // Secondary Button
  Serial.print(",");
  Serial.print(digitalRead(4)); // Joystick Button
  Serial.print(",");
  Serial.print(analogRead(A1)); // Joystick X
  Serial.print(",");
  Serial.print(analogRead(A0)); // Joystick Y
  Serial.print(",");
  Serial.print(analogRead(A2)); // Wheel Angle
  Serial.print(",");
  Serial.print(analogRead(A3)); // Clutch Angle
  Serial.print(",");
  Serial.print(analogRead(A4)); // Break Angle
  Serial.print(",");
  Serial.println(analogRead(A5)); // Accelerator Angle
  delay(20);
}
