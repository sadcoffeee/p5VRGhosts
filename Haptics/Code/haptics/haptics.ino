int btn = 2;
int motor = 3;

void setup() {
  pinMode(btn, INPUT);
  Serial.begin(9600);
  pinMode(motor,OUTPUT);

}


void loop() {
  int btnState = digitalRead(btn);
  Serial.println(btnState); // Debugging

  if (btnState == HIGH) {
    analogWrite(motor, 255); // PWM value
    Serial.print("pressed");
  } else {
    analogWrite(motor, 0);
  }

  delay(10);
}

