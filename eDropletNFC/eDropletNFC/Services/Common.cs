using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDropletNFC.Services
{
    public class Common
    {
        public static readonly byte[] sensor_L1 = new byte[3] { 0xDF, 0x00, 0x00 };      // Libre       
        public static readonly byte[] sensor_L2 = new byte[3] { 0xE5, 0x00, 0x03 };      // Libre US 14-day
        public static readonly byte[] sensor_L3 = new byte[3] { 0x9D, 0x08, 0x30 };      // Libre 2
        public static readonly byte[] sensor_L4 = new byte[3] { 0x70, 0x00, 0x10 };      // Libre Pro 
        public static readonly byte[] sensor_L5 = new byte[3] { 0x70, 0x00, 0x10 };      // Libre H   0x70 0x00 0x10 0x00 0x00 0x00 0xDD 0x01
        private static String SERIAL_NUMBER_ALPHABET = "0123456789ACDEFGHJKLMNPQRTUVWXYZ";

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:X2} ", b);
            return hex.ToString();
        }
        public static string ByteArrayToStringCont(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }
        public static string ByteArrayToStringCont1(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            int count = 0;
            foreach (byte b in ba)
            {
                if (count > 0 && count < 17)
                {
                    hex.AppendFormat("{0:X2}", b);
                }
                count++;
            }
            return hex.ToString();
        }
        public static String getSensorSerialNumber(int i, byte[] bArr)
        {
            if (i >= SERIAL_NUMBER_ALPHABET.Length || bArr == null || bArr.Length != 8)
            {
                return null;
            }
            int[] iArr = { bArr[5] & -1, bArr[4] & -1, bArr[3] & -1, bArr[2] & -1, bArr[1] & -1, bArr[0] & -1 };
            StringBuilder sb = new StringBuilder();
            sb.Append(SERIAL_NUMBER_ALPHABET[i]);
            sb.Append(SERIAL_NUMBER_ALPHABET[iArr[0] >> 3]);
            sb.Append(SERIAL_NUMBER_ALPHABET[(iArr[1] >> 6) | ((iArr[0] & 7) << 2)]);
            sb.Append(SERIAL_NUMBER_ALPHABET[(iArr[1] >> 1) & 31]);
            sb.Append(SERIAL_NUMBER_ALPHABET[((iArr[1] & 1) << 4) | (iArr[2] >> 4)]);
            sb.Append(SERIAL_NUMBER_ALPHABET[((iArr[2] & 15) << 1) | (iArr[3] >> 7)]);
            sb.Append(SERIAL_NUMBER_ALPHABET[(iArr[3] >> 2) & 31]);
            sb.Append(SERIAL_NUMBER_ALPHABET[((iArr[3] & 3) << 3) | (iArr[4] >> 5)]);
            sb.Append(SERIAL_NUMBER_ALPHABET[iArr[4] & 31]);
            sb.Append(SERIAL_NUMBER_ALPHABET[iArr[5] >> 3]);
            sb.Append(SERIAL_NUMBER_ALPHABET[(iArr[5] << 2) & 31]);

            return sb.ToString();
        }
        public static int checkSensorType(byte[] sensorTypeData)
        {
            byte[] std = new byte[3];
            for (int i = 0; i < 3; i++) std[i] = sensorTypeData[i];
            if (std.SequenceEqual(sensor_L1))
            {
                App.newSensorData.libreType = 1;
                App.newSensorData.libreTypeTxt = "Libre 1";
                return 0;
            }
            else if (std.SequenceEqual(sensor_L2))
            {
                App.newSensorData.libreType = 3;
                App.newSensorData.libreTypeTxt = "Libre US 14day";
                return 0;
            }
            else if (std.SequenceEqual(sensor_L3))
            {
                App.newSensorData.libreType = 2;
                App.newSensorData.libreTypeTxt = "Libre 2";
                return 3;
            }
            else if (std.SequenceEqual(sensor_L4))
            {
                App.newSensorData.libreType = 4;
                App.newSensorData.libreTypeTxt = "Libre Pro/H";
                return 0;
            }
            else if (std.SequenceEqual(sensor_L5))
            {
                App.newSensorData.libreType = 5;
                App.newSensorData.libreTypeTxt = "Libre H";
                return 0;
            }
            else
            {
                App.newSensorData.libreType = 0;
                return 9;
            }
        }
        public static string decodeStat(int stat)
        {
            String status = "---- ";
            switch (stat)
            {
                case 0:
                    status = "not initialized";
                    break;
                case 1:
                    status = "ready";
                    break;
                case 2:
                    status = "starting";
                    break;
                case 3:
                    status = "active";
                    break;
                case 4:
                    status = "shutting down";
                    break;
                case 5:
                    status = "expired";
                    break;
                case 6:
                    status = "failed";
                    break;
                default:
                    status = "N/A";
                    break;
            }
            return status;
        }
        public static string decodeStatL1(int stat)
        {
            String status = "---- ";
            switch (stat)
            {
                case 0xA0:
                    status = "not activated";
                    break;
                case 0xA4:
                    status = "starting";
                    break;
                case 0xA5:
                    status = "running";
                    break;
                case 0xA6:
                    status = "expiring";
                    break;
                case 0xA7:
                    status = "expired";
                    break;
                case 0xA8:
                    status = "broken";
                    break;
                default:
                    status = "N/A";
                    break;
            }
            return status;
        }
        public static int getGlucoseRaw(byte[] bytes, bool thirteen)
        {
            if (thirteen)
            {
                return ((256 * (bytes[1] & 0xFF) + (bytes[0] & 0xFF)) & 0x1FFF);
            }
            else
            {
                return ((256 * (bytes[1] & 0xFF) + (bytes[0] & 0xFF)) & 0x0FFF);
            }
        }
        public static int[] calcTrend(byte[] fram, int indexTrend)
        {
            int[] trend = new int[16];

            for (int index = 0; index < 16; index++)
            {
                int i = indexTrend - index;
                if (i < 0) i += 16;
                trend[index] = getGlucoseRaw(new byte[] { fram[(i * 6 + 28)], fram[(i * 6 + 29)] }, true);
            }
            return trend;
        }
        public static int[] calcHistory(byte[] fram, int indexHistory)
        {
            int[] trend = new int[32];

            for (int index = 0; index < 32; index++)
            {
                int i = indexHistory - index - 1;
                if (i < 0) i += 32;
                trend[index] = getGlucoseRaw(new byte[] { fram[(i * 6 + 124)], fram[(i * 6 + 125)] }, true);
            }
            return trend;
        }
        public static ushort[] crc16table = new ushort[256] { 0, 4489, 8978, 12955, 17956, 22445, 25910, 29887, 35912, 40385,
                                                44890, 48851, 51820, 56293, 59774, 63735, 4225, 264, 13203, 8730,
                                                22181, 18220, 30135, 25662, 40137, 36160, 49115, 44626, 56045, 52068,
                                                63999, 59510, 8450, 12427, 528, 5017, 26406, 30383, 17460, 21949,
                                                44362, 48323, 36440, 40913, 60270, 64231, 51324, 55797, 12675, 8202,
                                                4753, 792, 30631, 26158, 21685, 17724, 48587, 44098, 40665, 36688,
                                                64495, 60006, 55549, 51572, 16900, 21389, 24854, 28831, 1056, 5545,
                                                10034, 14011, 52812, 57285, 60766, 64727, 34920, 39393, 43898, 47859,
                                                21125, 17164, 29079, 24606, 5281, 1320, 14259, 9786, 57037, 53060,
                                                64991, 60502, 39145, 35168, 48123, 43634, 25350, 29327, 16404, 20893,
                                                9506, 13483, 1584, 6073, 61262, 65223, 52316, 56789, 43370, 47331,
                                                35448, 39921, 29575, 25102, 20629, 16668, 13731, 9258, 5809, 1848,
                                                65487, 60998, 56541, 52564, 47595, 43106, 39673, 35696, 33800, 38273,
                                                42778, 46739, 49708, 54181, 57662, 61623, 2112, 6601, 11090, 15067,
                                                20068, 24557, 28022, 31999, 38025, 34048, 47003, 42514, 53933, 49956,
                                                61887, 57398, 6337, 2376, 15315, 10842, 24293, 20332, 32247, 27774,
                                                42250, 46211, 34328, 38801, 58158, 62119, 49212, 53685, 10562, 14539,
                                                2640, 7129, 28518, 32495, 19572, 24061, 46475, 41986, 38553, 34576,
                                                62383, 57894, 53437, 49460, 14787, 10314, 6865, 2904, 32743, 28270,
                                                23797, 19836, 50700, 55173, 58654, 62615, 32808, 37281, 41786, 45747,
                                                19012, 23501, 26966, 30943, 3168, 7657, 12146, 16123, 54925, 50948,
                                                62879, 58390, 37033, 33056, 46011, 41522, 23237, 19276, 31191, 26718,
                                                7393, 3432, 16371, 11898, 59150, 63111, 50204, 54677, 41258, 45219,
                                                33336, 37809, 27462, 31439, 18516, 23005, 11618, 15595, 3696, 8185,
                                                63375, 58886, 54429, 50452, 45483, 40994, 37561, 33584, 31687, 27214,
                                                22741, 18780, 15843, 11370, 7921, 3960
        };
        public struct checkCRC
        {
            public ushort crc;
            public bool test;
        }
        checkCRC resultCRC = new checkCRC();
        public static ushort computeCRC16x(byte[] bytes, int len, int offset, ushort x)
        {
            int number_of_bytes_to_read = len;
            byte[] data = bytes;
            ushort crc = 0xffff;
            ushort reverseCrc = 0;
            for (int i = 0 + offset; i < number_of_bytes_to_read; ++i)
            {
                crc = (ushort)((crc >> 8) ^ crc16table[(crc ^ data[i]) & 0xff]);
            }
            for (int i = 0; i < 16; i++)
            {
                reverseCrc = (ushort)((ushort)(reverseCrc << 1) | (ushort)(crc & 1));
                crc >>= 1;
            }
            if (reverseCrc == x) return reverseCrc;
            else return reverseCrc;
        }
    }
}