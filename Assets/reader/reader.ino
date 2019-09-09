const int fsrReadingsPrSec = 20;
float timeStep;

const int a_FSRPin = A0;     // the left FSR and 10K pulldown are connected to a0

int fsrReading;

//int pressureThreshold;

int totalPressureLeft;
int totalPressureRight;
int dataPointsLeft;
int dataPointsRight;


void setup() {
  Serial.begin(9600);   
  while(!Serial){
    Serial.println("Waiting for Serial Port to open");
  }

  timeStep = 1000/fsrReadingsPrSec;
}
 
void loop() {
  fsrReading = analogRead(a_FSRPin);

  fsrReading = map(fsrReading, 0, 1023, 0, 255);

  //Used while hz is outcommented
  SendData(fsrReading);
  
  delay(timeStep);
}

void SendData(int lr){ /*, int rr*/
  Serial.write(lr);
}
