struct ControllerState{
  int a;
  int b;
  int wheel;
  int acltr;
  int brk;
};

ControllerState newState;
ControllerState oldState;

char serialOutput[20];

void setup() {
  Serial.begin(115200);
  pinMode(7, INPUT_PULLUP);
  pinMode(6, INPUT_PULLUP);
}

void loop() {
  newState.a = digitalRead(7);
  newState.b = digitalRead(6);
  newState.wheel = analogRead(A0);
  newState.acltr = analogRead(A2);
  newState.brk = analogRead(A4);

  if (newState.a != oldState.a ||
      newState.b != oldState.b ||
      abs(newState.wheel - oldState.wheel) > 5 ||
      abs(newState.acltr - oldState.acltr) > 5 ||
      abs(newState.brk - oldState.brk) > 5) {

    sprintf(serialOutput, "%d,%d,%d,%d,%d", newState.a, newState.b, newState.wheel, newState.acltr, newState.brk);

    Serial.println(serialOutput);

    oldState = newState;
  }

  //Serial.println(analogRead(A0));
}
