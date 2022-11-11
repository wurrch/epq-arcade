void setup() {
  Serial.begin(9600);      // Initialize the serial port and set the baud rate to 9600
  pinMode(10, INPUT_PULLUP); // Primary Button 
  pinMode(9, INPUT_PULLUP); // Secondary Button
  pinMode(8, INPUT_PULLUP); // Joystick Button
}

void loop() {
  // 'Primary Button','Secondary Button','Joystick X','Joystick Y','Joystick Button','Clutch','Break','Accelerator','Wheel Angle'
  Serial.print(digitalRead(10)); // Primary Button
  Serial.print(",");
  Serial.print(digitalRead(9)); // Secondary Button
  Serial.print(",");
  Serial.print(analogRead(A1)); // Joystick X
  Serial.print(",");
  Serial.print(analogRead(A0)); // Joystick Y
  Serial.print(",");
  Serial.print(digitalRead(8)); // Joystick Button
  Serial.print(",");
  Serial.print(analogRead(A2)); // Clutch Angle
  Serial.print(",");
  Serial.print(analogRead(A3)); // Break Angle
  Serial.print(",");
  Serial.print(analogRead(A4)); // Accelerator Angle
  Serial.print(",");
  Serial.println(analogRead(A5)); // Wheel Angle
  delay(20);
}
