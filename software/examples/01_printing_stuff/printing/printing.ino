void setup() {
  
  Serial.begin(115200);
  while (!Serial)
    delay(10); // will pause Zero, Leonardo, etc until serial console opens

  Serial.println("Hello from ESP 32!!!");
  // put your setup code here, to run once:
  delay(100);
}

void loop() {
  // put your main code here, to run repeatedly:
  Serial.println("I'm running!!!");
  delay(5000);
}
