using System.ComponentModel;
using System.Reflection;

namespace Pos.Domain.ValueObjects
{
    public class Enums
    {
        public enum StatusCodes
        {

            [Description("Fiscal Invoice Number generated successfully.")]
            Code_100 = 100,

            [Description("Error while generating Fiscal Invoice Number.")]
            Code_101 = 101,

            [Description("Summary record received successfully")]
            Code_200 = 200,

            [Description("Error while receiving Summary record")]
            Code_201 = 201,

            [Description("Some error has occured. Please contact your service provider.")]
            Code_400 = 400,

            [Description("Invalid data received.")]
            Code_401 = 401,

            [Description("Model validation failed.")]
            Code_402 = 402,


        }

        public enum InvoiceStatus
        {
            NotSynced = 0,
            Synced = 1,
            Faulty = 2
        }

        public enum Sync
        {
            POSStatus = 1,
            Invoice = 2
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
    public enum EnvironmentType
    {
        Sandbox,
        Production
    }
}
