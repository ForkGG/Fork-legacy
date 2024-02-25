using System.Collections.Generic;
using System.Text;

namespace Fork.Logic.Utils;

public class ByteUtils
{
    public static List<string> SplitByteArray(byte[] input)
    {
        // Limpiamos el inicio y el final de caracteres nulos
        input = CleanByteArray(input);

        List<string> output = new();
        List<byte> bytes = new();

        foreach (byte bInput in input)
            if (bInput == 0x00)
            {
                string data = string.Empty;
                if (bytes.Count > 0)
                {
                    data = Encoding.UTF8.GetString(bytes.ToArray()).Trim();
                }

                output.Add(data);
                bytes = new List<byte>();
            }
            else
            {
                bytes.Add(bInput);
            }

        return output;
    }

    public static byte[] CleanByteArray(byte[] inputBytes)
    {
        // Variables de indices
        int firstIndex = 0;
        int lastIndex = inputBytes.Length;

        // Buscamos el primer cáracter no nulo
        for (int i = 0; i < inputBytes.Length; i++)
        {
            if (inputBytes[i] == 0x00)
            {
                continue;
            }

            firstIndex = i;
            break;
        }

        // Buscamos el último cáracter no nulo
        for (int i = inputBytes.Length - 1; i >= 0; i--)
        {
            if (inputBytes[i] == 0x00)
            {
                continue;
            }

            lastIndex = i + 1;
            break;
        }

        return SubByteArray(inputBytes, firstIndex, lastIndex);
    }

    public static byte[] SubByteArray(byte[] inputBytes, int startIndex, int lastIndex)
    {
        if (lastIndex - startIndex > inputBytes.Length)
        {
            return inputBytes;
        }

        byte[] outputBytes = new byte[lastIndex - startIndex + 1];

        for (int i = startIndex; i <= lastIndex; i++)
        {
            outputBytes[i - startIndex] = inputBytes[i];
        }

        return outputBytes;
    }
}