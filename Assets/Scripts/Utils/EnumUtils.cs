using System;

namespace Utils {
    public static class EnumUtils {
        public static T GetRandomEnumValue<T>() where T : Enum {
            Array values = Enum.GetValues(typeof(T));
            int randomIndex = UnityEngine.Random.Range(0, values.Length);
            return (T) values.GetValue(randomIndex);
        }
    }
}