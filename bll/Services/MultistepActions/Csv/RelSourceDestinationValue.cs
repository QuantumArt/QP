using System.Linq;
using System;
using System.Text;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class RelSourceDestinationValue
    {
        public int OldId { get; set; }

        public int NewId { get; set; }

        public int FieldId { get; set; }

        public int[] NewRelatedItems { get; set; }

        public bool IsM2M { get; set; }

        public RelSourceDestinationValue()
        {

        }

        public RelSourceDestinationValue(string line)
        {
            string[] values = line.Split(";");

            if (values.Length != 5)
            {
                throw new InvalidOperationException($"Less elements then expected in line {line}");
            }

            OldId = TryParseToInt(values[0]);
            NewId = TryParseToInt(values[1]);
            FieldId = TryParseToInt(values[2]);
            NewRelatedItems = TryParseStringToArray(values[3]);

            if (!bool.TryParse(values[4], out bool isM2M))
            {
                throw new InvalidOperationException($"Unable parse {values[4]} to int.");
            }
            IsM2M = isM2M;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            _ = sb.Append(OldId);
            _ = sb.Append(';');
            _ = sb.Append(NewId);
            _ = sb.Append(';');
            _ = sb.Append(FieldId);
            _ = sb.Append(';');
            _ = sb.Append(string.Join(',', NewRelatedItems));
            _ = sb.Append(';');
            _ = sb.Append(IsM2M);
            return sb.ToString();
        }

        private static int[] TryParseStringToArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<int>();
            }

            string[] values = value.Split(",");

            return values
                .Select(x => TryParseToInt(x))
                .ToArray();
        }

        private static int TryParseToInt(string value)
        {
            return !int.TryParse(value, out int intValue)
                ? throw new InvalidOperationException($"Unable parse {value} to int.")
                : intValue;
        }
    }
}
