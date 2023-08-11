using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Permissions;

namespace DQ8Rando3DS.Tables
{
    public class MessageFile
    {
        public byte[] Header { get; set; }
        public uint[] Pointers { get; set; }
        public string[] Strings { get; set; }

        public MessageFile(byte[] data)
        {
            int last_end_index = -1;
            int penultimate_end_index = -1;
            for (int i = data.Length - 5; i >= 0; i--)
            {
                if (Encoding.UTF8.GetString(data[i..(i + 5)]) == "[end]")
                {
                    if (last_end_index < 0)
                    {
                        last_end_index = i;
                        continue;
                    }
                    else
                    {

                        penultimate_end_index = i;
                        break;
                    }
                }
            }

            if (last_end_index >= 0 && penultimate_end_index >= 0)
            {
                int last_string_index = penultimate_end_index + 5;

                int last_pointer_index = -1;
                int first_string_index = -1;
                int first_pointer_index = -1;
                for (int i = data.Length - data.Length % 4; i >= 0; i -= 4)
                {
                    if (i + 4 >= data.Length)
                        continue;

                    if (BitConverter.ToUInt32(data[i..(i+4)]) == last_string_index)
                    {
                        last_pointer_index = i;
                        first_string_index = i + 4;
                    }
                    if (first_string_index >= 0 && BitConverter.ToUInt32(data[i..(i+4)]) == first_string_index)
                    {
                        first_pointer_index = i;
                        break;
                    }
                }

                List<uint> pointers = new List<uint>();
                for (int i = first_pointer_index; i <= last_pointer_index; i += 4)
                {
                    pointers.Add(BitConverter.ToUInt32(data[i..(i+4)]));
                }
                Pointers = pointers.ToArray();

                Header = data[..first_pointer_index];
            }
            if (last_end_index >= 0 && penultimate_end_index < 0)
            {
                Header = data[..0x10];
                Pointers = new uint[] { 0x14 };
            }

            Strings = new string[Pointers.Length];
            Regex pattern = new Regex(".*?(?=\\[end\\])", RegexOptions.Multiline);
            for (int i = 0; i < Pointers.Length; i++)
            {
                Strings[i] = pattern.Match(Encoding.UTF8.GetString(data[(int)Pointers[i]..])).Value;
            }
        }
        public MessageFile(byte[] header, uint[] pointers, string[] strings)
        {
            Header = header;
            Pointers = pointers;
            Strings = strings;
        }
        public MessageFile Clone()
        {
            return new MessageFile(Header[..], Pointers[..], Strings[..]);
        }
        public static MessageFile Load(string path)
        {
            return new MessageFile(File.ReadAllBytes(path));
        }
        public void Save(string path)
        {
            IEnumerable<byte> data = Header.ToList();

            UpdatePointers();
            foreach (uint pointer in Pointers)
            {
                data = data.Concat(BitConverter.GetBytes(pointer));
            }
            foreach (string str in Strings)
            {
                data = data.Concat(Encoding.UTF8.GetBytes(str + "[end]"));
            }

            Extensions.WriteAllBytes(path, data.ToArray());
        }
        public void UpdatePointers()
        {
            int byte_count = Header.Length + (Pointers.Length * 4);

            for (int i = 0; i < Pointers.Length; i++)
            {
                Pointers[i] = (uint)byte_count;
                byte_count += Encoding.UTF8.GetBytes(Strings[i] + "[end]").Length;
            }
        }
    }
}
