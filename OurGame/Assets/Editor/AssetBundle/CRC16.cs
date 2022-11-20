using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class CRC16
{
    public static string ToCRC16(string content)
    {
        byte[] _buff = Encoding.UTF8.GetBytes(content);
        byte[] _crc = GetCRC16(_buff);
        StringBuilder _sb = new StringBuilder();
        foreach (byte _b in _crc)
        {
            _sb.AppendFormat("{0:x}",_b);
        }
        //return Encoding.UTF8.GetString(_crc);
        return _sb.ToString();
    }

    static byte[] GetCRC16(byte[] data)
    {
        int len = data.Length;
        if (len > 0)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < len; i++)
            {
                crc = (ushort)(crc ^ data[i]);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    }
                    else
                    {
                        crc = (ushort)(crc >> 1);
                    }
                }
            }

            byte high = (byte)((crc & 0xFF00) >> 8);
            byte low = (byte)(crc & 0x00FF);
            return new byte[] { high, low };
        }
        return new byte[] { 0, 0 };
    }
}
