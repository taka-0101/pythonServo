using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using Extensions.Collections;

namespace MyDll
{
    public class Class1
    {
        static SerialPort serialPort1;

        public Class1(string com)
        {
            if ((serialPort1 != null))
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                }
            }
            // serialPortの設定
            serialPort1 = new SerialPort();
            serialPort1.BaudRate = 1500000;
            serialPort1.PortName = com;
            serialPort1.ReadTimeout = 500;
            Class1_Load();
        }

        ~Class1()
        {
            serialPort1.Close();
            Console.WriteLine("ポートを閉じました");
        }

        /// <summary>
        /// フォームが立ち上がった時に呼ばれるイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool Class1_Load()
        {
            
            try{
                serialPort1.Open();             //COMポートを開く
                Console.WriteLine("接続に成功しました");
                return true;
            }
            catch ( Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("接続に失敗しました");
                serialPort1.Close();
                return false;
                
            }
        }

        public void Class1_Del()
        {
            serialPort1.Close();
            
            Console.WriteLine("ポートを閉じました");
        }

        

        
        #region B3Mの動作関数

        /// <summary>
        /// トルクオン、位置制御モード等々一度に設定する
        /// </summary>
        /// <param name="serialPort"></param>
        /// <param name="servoID"></param>
        /// <returns></returns>
        private bool b3mModeSet(SerialPort serialPort, byte servoID, byte mode)
        {
            ByteList cmd = new ByteList();  //コマンド格納用
            byte[] rx = new byte[5];        //コマンド受信用(Writeコマンドは5byte)

            //コマンドの作成
            //cmd.Bytes = B3MLib.B3MLib.Write(0, B3MLib.B3MLib.SERVO_TORQUE_ON, 1, servoID, new byte[] { mode });
            cmd.Bytes = B3MLib.B3MLib.WriteSingle(0, B3MLib.B3MLib.SERVO_TORQUE_ON, servoID, new byte[] { mode });
            
            //option:0
            //address:B3MLib.B3MLib.SERVO_TORQUE_ON (0x28)
            //count:1(データを送るB3Mの数)
            //ID:servoID
            //data:送信するデータ配列

            // コマンドを送信
            return B3MLib.B3MLib.Synchronize(serialPort, cmd.Bytes,ref rx);
        }


        /// <summary>
        /// Nomal、位置制御モードにする
        /// </summary>
        /// <param name="serialPort1">serialport</param>
        /// <param name="servoID">id</param>
        /// <returns></returns>
        private bool b3mNomalPosModeSet(SerialPort serialPort, byte servoID)
        {
            return b3mModeSet(serialPort, servoID, ((byte)B3MLib.B3MLib.Options.ControlPosition | (byte)B3MLib.B3MLib.Options.RunNormal));
        }

        /// <summary>
        /// Free、位置制御モードにする
        /// </summary>
        /// <param name="serialPort1">serialport</param>
        /// <param name="servoID">id</param>
        /// <returns></returns>
        private bool b3mFreePosModeSet(SerialPort serialPort, byte servoID)
        {
            return b3mModeSet(serialPort, servoID, ((byte)B3MLib.B3MLib.Options.ControlPosition | (byte)B3MLib.B3MLib.Options.RunFree));
        }

        /// <summary>
        /// Hold、位置固定モードにする
        /// </summary>
        /// <param name="serialPort1">serialport</param>
        /// <param name="servoID">id</param>
        /// <returns></returns>
        private bool b3mHoldPosModeSet(SerialPort serialPort, byte servoID)
        {
            return b3mModeSet(serialPort, servoID, ((byte)B3MLib.B3MLib.Options.ControlPosition | (byte)B3MLib.B3MLib.Options.RunHold));
        }

        /// <summary>
        /// 角度指定をする関数
        /// </summary>
        /// <param name="serialPort"></param>
        /// <param name="servoID"></param>
        /// <param name="angle"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool b3mSetPosition(SerialPort serialPort, byte servoID, int angle, ushort time)
        {
            ByteList cmd = new ByteList();  //コマンド格納用
            byte[] rx = new byte[7];        //コマンド受信用(Writeコマンドは7byte)

            //コマンドの作成
            cmd.Bytes = B3MLib.B3MLib.SetPosision(0, servoID, angle, time);
            //option:0
            //ID:servoID
            //pos:送信するデータ配列
            //time:軌道生成時の時間

            // コマンドを送信
            return B3MLib.B3MLib.Synchronize(serialPort, cmd.Bytes, ref rx);
        }


        /// <summary>
        /// サーボの現在角度を読み込む
        /// </summary>
        /// <param name="servoID"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        static public bool b3mAngleRead(SerialPort serialPort, byte servoID, ref int angle)
        {
            ByteList cmd = new ByteList();
            byte[] rx = new byte[7];        //コマンド受信用(Readで2byte受け取るコマンドは7byte)

            //コマンドの作成
            cmd.Bytes = B3MLib.B3MLib.Read(0x00, B3MLib.B3MLib.SERVO_CURRENT_POSITION, 2, servoID);
            //option:0
            //address:B3MLib.B3MLib.SERVO_CURRENT_POSITION (0x2C)
            //count:2(受け取るデータの数)
            //ID:servoID


            // コマンドを送信
            if (B3MLib.B3MLib.Synchronize(serialPort, cmd.Bytes, ref rx) == false)
            {
                return false;
            }

            //取得したデータをint(short)型に変換
            angle = (short)Extensions.Converter.ByteConverter.ByteArrayToInt16(rx[4], rx[5]);

            return true;
        }


        #endregion

        #region ボタンの操作

        /// <summary>
        /// フリーボタンがクリックされたとき呼ばれる関数イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool freeButton_Click(byte servoID)
        {
            bool flag;

            flag = b3mFreePosModeSet(serialPort1,servoID);

            if (flag == false)
            {
                Console.WriteLine("データの送信に失敗しました");
            }
            return flag;

        }
        /// <summary>
        /// ノーマルボタンがクリックされたときに呼ばれるイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool normalButton_Click(byte servoID)
        {
            bool flag;

            flag = b3mNomalPosModeSet(serialPort1, servoID);

            if (flag == false)
            {
                Console.WriteLine("データの送信に失敗しました");
            }
            return flag;
        }
        /// <summary>
        /// ホールドボタンがクリックされたときに呼ばれるイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool holdButton_Click(byte servoID)
        {
            bool flag;

            flag = b3mHoldPosModeSet(serialPort1, servoID);

            if (flag == false)
            {
                Console.WriteLine("データの送信に失敗しました");
            }
            return flag;
        }

        /// <summary>
        /// setPosボタンが押されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void setPosButton_Click(int servoID, int angle, int time)
        {
            bool flag;

            flag = b3mSetPosition(serialPort1, (byte)servoID, (int)angle, (ushort)time);

            if (flag == false)
            {
                Console.WriteLine("データの送信に失敗しました");
            }
        }

        /// <summary>
        /// getPosボタンが押されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public int getPosButton_Click(int servoID)
        {
            bool flag;

            int angle = new int();

            flag = b3mAngleRead(serialPort1, (byte)servoID, ref angle);

            
            if (flag == false)
            {
                Console.WriteLine("データの送信に失敗しました");
                return 0;
            }

            return angle;
        }

        #endregion
    }
    

}
