using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrographicsSample
{
    internal static class SettingsSaver
    {
        public static void SaveSettings(object instance, string filename)
        {
            var props = instance.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.GetProperty);
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename))
            {
                foreach (var item in props)
                {
                    if (!item.PropertyType.IsValueType)
                        continue; //TODO
                    var value = item.GetValue(instance);
                    sw.WriteLine($"{item.Name}\t{value}");
                }
            }
        }

        public static bool LoadSettings(object instance, string filename)
        {
            if (!System.IO.File.Exists(filename))
                return false;
            var props = instance.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.GetProperty);
            foreach (string line in System.IO.File.ReadAllLines(filename))
            {
                var pairs = line.Split('\t');
                var prop = props.Where(p => p.Name == pairs[0]).FirstOrDefault();
                object value = pairs[1];
                if(prop != null)
                {
                    if (!prop.PropertyType.IsValueType)
                        continue; //TODO
                    if (prop.SetMethod == null)
                        continue;
                    if (prop.PropertyType.IsEnum)
                        value = Enum.Parse(prop.PropertyType, (string)value);
                    prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
                }
            }
            return true;
        }
    }
}
