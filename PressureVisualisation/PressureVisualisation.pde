import hypermedia.net.*;

int pressureUDP_PORT = 50000;
int conductanceUDP_PORT = 51000;
String CONNECTION_IP = "224.0.0.200";
UDP pressureUDP, ConductanceUDP;

int positionIndex;

int[] pressureValue;
int[] conductanceValue;



void setup(){
  size(600, 300);
  
  pressureUDP = new UDP(this, pressureUDP_PORT, CONNECTION_IP);
  pressureUDP.log(true);
  pressureUDP.listen(true);
  
  ConductanceUDP = new UDP(this, conductanceUDP_PORT, CONNECTION_IP);
  ConductanceUDP.log(true);
  ConductanceUDP.listen(true);
  
  pressureValue = new int[2];
  conductanceValue = new int[2];
  
  textSize(16);
  
  ResetSketch();;
  
  noSmooth();
  noLoop();
}

void draw(){
  if(positionIndex > width){
    ResetSketch();
  }
  
  stroke(0,255,255);
  line(  positionIndex-1,
         height/2 - pressureValue[1], 
         positionIndex, 
         height/2 - pressureValue[0]
         );
         
  stroke(200,255,100);
  line(  positionIndex-1,
         height/2 - conductanceValue[1] + height/2, 
         positionIndex, 
         height/2 - conductanceValue[0] + height/2
       );
}

void receive(byte[] data, String host_IP, int port_RX){
  
  if(port_RX == pressureUDP_PORT+1){
    pressureValue[1] = pressureValue[0];
    pressureValue[0] = Integer.parseInt(new String(data));
    pressureValue[0] = NormalizeNumbers(pressureValue[0], 0, 255, 0, height/2-40);
    positionIndex++;
    
  } else if (port_RX == conductanceUDP_PORT+1){
    conductanceValue[1] = conductanceValue[0];
    conductanceValue[0] = Integer.parseInt(new String(data));
    conductanceValue[0] = (int)(conductanceValue[0] / 9.8 * 1000);
    conductanceValue[0] = NormalizeNumbers(conductanceValue[0], 0, 45000, 0, height/2-40);
    positionIndex++;
    
  } else {
    println("Received unknown package: " + new String(data) + ". On IP: " + host_IP + " on port: " + port_RX);
  }
  
  redraw();
}

void ResetSketch(){
  background(0);
  
  stroke(100);
  for(int i = height; i > 0; i-=10){
    line(0,i,width,i);
  }
  
  fill(0,255,255);
  textSize(16);
  textFont(createFont("Arial bold", 20));
  text("Analog signal:", 5, 20);
  fill(200,255,100);
  text("Conductance:", 5, 20+height/2);
  
  stroke(255);
  line(0, height/2, width, height/2);
  positionIndex = 0;
}

int NormalizeNumbers(float input, float inMin, float inMax, float outMin, float outMax){
  return round(((input - inMin) * (outMax - outMin)) / (inMax - inMin) + outMin);
}
