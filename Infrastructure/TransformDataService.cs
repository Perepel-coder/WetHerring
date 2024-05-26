using System.Data;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Infrastructure
{
    public static class TransformDataService
    {
        public static Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                //return Encoding.GetEncoding("windows-1251");
                return Encoding.UTF8;
            }
        }

        public static IEnumerable<byte> ToTypeByte(this DataTable table)
        {
            using MemoryStream ms = new();
            table.WriteXml(ms);
            return ms.ToArray();
        }
        public static IEnumerable<byte> ToTypeByte(this string data)
        {
            return Encoding.GetBytes(data);
        }
        public static IEnumerable<byte> ToTypeByte(this IEnumerable<int> data)
        {
            return data.Select(el => (byte)el);
        }
        public static IEnumerable<int> ToTypeInt(this string data)
        {
            return Encoding.GetBytes(data).Select(i => (int)i);
        }
        public static IEnumerable<int> ToTypeInt(this IEnumerable<byte> data)
        {
            return data.Select(i => (int)i);
        }
        public static IEnumerable<int> ToTypeInt(this DataTable table)
        {
            return table.ToTypeByte().ToTypeInt();
        }
        public static string ToTypeString(this IEnumerable<byte> data)
        {
            return Encoding.GetString(data.ToArray());
        }
        public static XDocument ToTypeXML<Key, Value>(this Dictionary<Key, Value> elements, string rootName = "root") where Key : notnull
        {
            XDocument xdoc = new();
            XElement root = new(rootName);
            foreach (var el in elements)
            {
                XElement element = new("parameter");
                element.Add(new XAttribute("name", el.Key));
                element.Add(new XElement("value", el.Value));
                root.Add(element);
            }
            xdoc.Add(root);
            return xdoc;
        }
        public static Dictionary<string, string> ToTypeDictionary(this XDocument xdoc, string rootName = "root")
        {
            Dictionary<string, string> pairs = new();
            XElement? root = xdoc.Element(rootName);
            if (root == null)
            {
                throw new Exception($"Корневой элемент {rootName} не найден.");
            }
            foreach (var el in root.Elements("parameter"))
            {
                pairs.Add(el.Attribute("name").Value, el.Element("value").Value);
            }
            return pairs;
        }

        public static string ConvertToString2(this uint num)
        {
            string result = string.Empty;

            while (num != 0)
            {
                ulong nextDigit = num & 0x01;
                result = nextDigit + result;
                num >>= 1;
            }
            return result;
        }

        public static int ConvertTo_10_From_2(this string num)
        {
            int num10 = 0;
            byte[] chars = num.Reverse().Select(_char => (byte)(_char - '0')).ToArray();

            for (byte index = 0; index < chars.Length; index++)
            {
                num10 += chars[index] * index;
            }

            return num10;
        }
    }
}
