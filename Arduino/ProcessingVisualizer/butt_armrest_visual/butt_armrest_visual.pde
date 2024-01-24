import processing.serial.*;
int[] a=new int[51];
int thres=0,thresh_butt=300;
Serial myPort;
String val,inString;
int lf=10,l=0,b=0;
String[] list;
void setup()
{
  size(2000,2000);
 String portName="COM3";
 myPort=new Serial(this,portName,115200);
 myPort.bufferUntil(lf);
 
   


  
}
void draw()
{
  //print(inString);
  //print(inString);
   if(inString!="null")
   {
  list=split(inString,',');
 //println(list.length);

  try {
    if(list.length==46)
    {
     // print(list);
   //println(list[0]);
   //buttfsr
    if(int(list[0])>thresh_butt+200){ a[0]=0;} else {a[0]=255;}
    if(int(list[1])>thresh_butt){ a[1]=0;} else {a[1]=255;}
    if(int(list[2])>thresh_butt){ a[2]=0;} else {a[2]=255;}
    if(int(list[3])>thresh_butt){ a[3]=0;} else {a[3]=255;}
    if(int(list[4])>thresh_butt){ a[4]=0;} else {a[4]=255;}
    
    //lower back fsr
    if(int(list[5])>thresh_butt){ a[5]=0;} else {a[5]=255;}
    if(int(list[15])>thresh_butt){ a[6]=0;} else {a[6]=255;}
    if(int(list[7])>thresh_butt){ a[7]=0;} else {a[7]=255;}
    if(int(list[8])>thresh_butt){ a[8]=0;} else {a[8]=255;}
    if(int(list[9])>thresh_butt){ a[9]=0;} else {a[9]=255;}
    
    
    //upper back
    if(int(list[10])>thresh_butt){ a[10]=0;} else {a[10]=255;}
    if(int(list[11])>thresh_butt){ a[11]=0;} else {a[11]=255;}
    if(int(list[12])>thresh_butt){ a[12]=0;} else {a[12]=255;}
    if(int(list[13])>thresh_butt){ a[13]=0;} else {a[13]=255;}
    if(int(list[14])>thresh_butt+300){ a[14]=0;} else {a[14]=255;}
    
    //left touch sensor
    if(int(list[16])>thres){ a[16]=0;} else {a[16]=255;}
    if(int(list[17])>thres){ a[17]=0;} else {a[17]=255;}
    if(int(list[18])>thres){ a[18]=0;} else {a[18]=255;}
    if(int(list[19])>thres){ a[19]=0;} else {a[19]=255;}
    if(int(list[20])>thres){ a[20]=0;} else {a[20]=255;}
    if(int(list[21])>thres){ a[21]=0;} else {a[21]=255;}
    if(int(list[22])>thres){ a[22]=0;} else {a[22]=255;}
    if(int(list[23])>thres){ a[23]=0;} else {a[23]=255;}
    if(int(list[24])>thres){ a[24]=0;} else {a[24]=255;}
    if(int(list[25])>thres){ a[25]=0;} else {a[25]=255;}
    if(int(list[26])>thres){ a[26]=0;} else {a[26]=255;}
    if(int(list[27])>thres){ a[27]=0;} else {a[27]=255;}
       
       
    //right touch sensor
    if(int(list[28])>thres){ a[28]=0;} else {a[28]=255;}
    if(int(list[29])>thres){ a[29]=0;} else {a[29]=255;}
    if(int(list[30])>thres){ a[30]=0;} else {a[30]=255;}
    if(int(list[31])>thres){ a[31]=0;} else {a[31]=255;}
    if(int(list[32])>thres){ a[32]=0;} else {a[32]=255;}
    if(int(list[33])>thres){ a[33]=0;} else {a[33]=255;}
    if(int(list[34])>thres){ a[34]=0;} else {a[34]=255;}
    if(int(list[35])>thres){ a[35]=0;} else {a[35]=255;}
    if(int(list[36])>thres){ a[36]=0;} else {a[36]=255;}
    if(int(list[37])>thres){ a[37]=0;} else {a[37]=255;}
    if(int(list[38])>thres){ a[38]=0;} else {a[38]=255;}
    if(int(list[39])>thres){ a[39]=0;} else {a[39]=255;}

    

    //Right and Left Arm rest FSR sensors
    if(int(list[40])>thresh_butt){ a[40]=0;} else {a[40]=255;}
    if(int(list[41])>thresh_butt){ a[41]=0;} else {a[41]=255;}
    
    //Servo top and bottom servo angles
    a[42]=int(list[42]);
    a[43]=int(list[43]);
    
    //back tilt value
     a[44]=int(list[44]);    


    


    
    
    }
  }
catch(NullPointerException e)
{
} finally{
}
}

//fill(a[20]);
//right touch sensor
write_text("Right touch sensor",0,100);
drawrect(0,150,100,40,a[33]);
drawrect(0,250,100,40,a[32]);
drawrect(0,350,100,40,a[31]);
drawrect(0,450,100,40,a[30]);
drawrect(0,550,100,40,a[29]);
drawrect(0,650,100,40,a[28]);


drawrect(200,150,100,40,a[34]);
drawrect(200,250,100,40,a[35]);
drawrect(200,350,100,40,a[36]);
drawrect(200,450,100,40,a[37]);
drawrect(200,550,100,40,a[38]);
drawrect(200,650,100,40,a[39]);


//left touch sensor
write_text("Left touch sensor",0,800);
drawrect(0,850,100,40,a[21]);
drawrect(0,950,100,40,a[20]);
drawrect(0,1050,100,40,a[19]);
drawrect(0,1150,100,40,a[18]);
drawrect(0,1250,100,40,a[17]);
drawrect(0,1350,100,40,a[16]);

drawrect(200,850,100,40,a[22]);
drawrect(200,950,100,40,a[23]);
drawrect(200,1050,100,40,a[24]);
drawrect(200,1150,100,40,a[25]);
drawrect(200,1250,100,40,a[26]);
drawrect(200,1350,100,40,a[27]);


//butt fsr sensor
write_text("Butt FSRs",700,100);
drawrect(500,150,100,100,a[4]);
drawrect(700,150,100,100,a[3]);//4
drawrect(900,150,100,100,a[0]);
drawrect(500,350,100,100,a[2]); //2
drawrect(900,350,100,100,a[1]); //3

//lower back
write_text("Lower Back FSRs",750,600);
drawrect(500,700,100,100,a[9]);
drawrect(700,700,100,100,a[8]);//4
drawrect(900,700,100,100,a[7]);
drawrect(1100,700,100,100,a[6]); //2
drawrect(1300,700,100,100,a[5]); //3


//upperback -array values needs to be updated
write_text("Upper Back FSRs",750,900);
drawrect(500,1000,100,100,a[14]);
drawrect(700,1000,100,100,a[13]);//4
drawrect(900,1000,100,100,a[12]);
drawrect(1100,1000,100,100,a[11]); //2
drawrect(1300,1000,100,100,a[10]); //3


//arm rst fsr -array values need to be updated
write_text("Left Arm",500,1300);
drawrect(800,1350,100,100,a[40]);

write_text("Right Arm",800,1300);
drawrect(500,1350,100,100,a[41]);//4


write_text("Backtilt=",500,1550);
fill(255);
rect(800,1500,100,100);
write_text(nf(a[44],4),800,1550);

write_text("Servo Top Angle=",500,1700);
fill(255);
rect(800,1650,100,100);
write_text(nf(a[42],3),800,1700);

write_text("Servo Bottom Angle=",500,1850);
fill(255);
rect(900,1800,100,100);
write_text(nf(a[43],3),900,1850);
//l=100;
//b=100;
//int x=210;
//fill(a[0]);
//rect(x+50,10,l,b);
//fill(a[1]);
//rect(50+x+l+50,10,l,b);



//int y=200;
//x=210;
//fill(a[4]);
//rect(x,y,l,b);
//fill(a[3]);
//rect(x+l+50,y,l,b);
//fill(a[2]);
//rect(x+l*2+100,y,l,b);


//y=400;
//x=210;
//fill(a[5]);
//rect(x,y,l,b);
//fill(a[6]);
//rect(x,y+50+b,l,b);
//fill(a[7]);
//rect(x,y+100+2*b,l,b);



//fill(a[2]);

//rect(250,10,l,b);
//fill(a[4]);
//rect(10,b+20,l,b);
//fill(a[3]);
//rect(250,b+20,l,b);


}
void write_text(String s,int x1,int y1){
  textSize(32);
  fill(0);
  text(s,x1,y1);
}
void drawrect(int x1,int y1,int w,int h,int status){
if(status==0){
  fill(255,0,0);  
}
else
{
  fill(255,255,255);
}
rect(x1,y1,w,h);
}

void serialEvent(Serial p)
{
 inString =p.readString(); 
}
