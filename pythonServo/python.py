

import clr
from time import sleep

clr.AddReference("MyDll")
from MyDll import Class1

servo = Class1("COM4")
while True :
    if servo.getPosButton_Click(1) > -7000 or servo.getPosButton_Click(1) < -9000:
        servo.normalButton_Click(1)
        servo.setPosButton_Click(1,-8000,0)
        print(servo.getPosButton_Click(1))
    else:
        servo.holdButton_Click(1)
    

sleep(1)
servo.freeButton_Click(1)