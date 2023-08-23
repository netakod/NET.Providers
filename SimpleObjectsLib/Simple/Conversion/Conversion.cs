using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public static class Conversion 
    {
        public static T TryChangeType<T>(object? value)
        {
            return TryChangeType<T>(value, () => default);
        }

        public static T TryChangeType<T>(object? value, T defaultValue)
        {
            return TryChangeType<T>(value, () => defaultValue);
        }

        public static T TryChangeType<T>(object? value, Func<T> getDefaultValue)
        {
            //object resultObject = TryChangeType(value, typeof(T));
            T result;
            object? resultObject = null;

            if (value == null || Convert.IsDBNull(value))
            {
                result = getDefaultValue();
            }
            else if (value.GetType() == typeof(T))
            {
                result = (T)value;
            }
            else
            {
                resultObject = TryChangeType(value, typeof(T), getDefaultValue);

                try
                {
                    result = (T)resultObject;
                }
                catch
                {
                    result = getDefaultValue();
                }

                if (result == null && value != null)
                {
                    try
                    {
                        result = (T)value;
                    }
                    catch
                    {
                    }
                }
            }

            return result;
        }

        public static object? TryChangeType(object? value, int declaredTypeId)
		{
            Type declaredType = PropertyTypes.GetPropertyType(declaredTypeId);

            return TryChangeType(value, declaredType);
        }

        public static object? TryChangeType(object? value, int declaredTypeId, object? defaultValue)
        {
            Type declaredType = PropertyTypes.GetPropertyType(declaredTypeId);

            return TryChangeType(value, declaredType, defaultValue);
        }

        public static object? TryChangeType(object? value, Type declaredType)
        {
            return TryChangeType(value, declaredType, () => ReflectionHelper.GetDefaultValue(declaredType));
        }

        public static object? TryChangeType(object? value, Type declaredType, object? defaultValue)
        {
            return TryChangeType(value, declaredType, () => defaultValue);
        }

        public static object? TryChangeType(object? value, Type declaredType, Func<object?> getDefaultValue)
        {
            object? result = value;

            if (declaredType != null)
            {
                if (value == null || Convert.IsDBNull(value))
                {
                    result = getDefaultValue();
                }
                else
                {
                    if (declaredType == typeof(object))
                    {
                        result = value;
                    }
                    else if (declaredType == typeof(int) && (value is string))
                    {
                        int intResult;

                        if (int.TryParse(value as string, out intResult))
                        {
                            result = intResult;
                        }
                        else
                        {
                             result = getDefaultValue();
                        }
                    //    catch
                    //    {
                    //        result = getDefaultValue();
                    //    }
                    //}
                    //else if (declaredType == typeof(long))
                    //{
                    //    try
                    //    {
                    //        result = Convert.ToInt64(value);
                    //    }
                    //    catch
                    //    {
                    //        result = getDefaultValue();
                    //    }
                    }
                    else if (declaredType.IsEnum)
                    {
                        string? str = value.ToString();

                        if (str == null)
                        {
                            result = getDefaultValue();
                        }
                        else
                        {
                            try
                            {
                                result = Enum.Parse(declaredType, str);
                            }
                            catch
                            {
                                result = getDefaultValue();
                            }
                        }
                    }
                    else if (declaredType == typeof(String))
                    {
                        string? str = value.ToString();

                        if (value != null && str != null && str.Trim().Length > 0)
						{
							result = str;
						}
						else
						{
							result = getDefaultValue();
						}
                    }
					else
                    {
						Type? underlyingType = Nullable.GetUnderlyingType(declaredType);

						if (underlyingType != null)
						{
							if (value == null)
								return getDefaultValue();

							declaredType = underlyingType;
						}

						try
						{
                            result = Convert.ChangeType(value, declaredType);
                        }
                        catch
                        {
                            result = getDefaultValue();
                        }
                    }
                }
            }
            else
            {
                result = getDefaultValue();
            }

            return result;
        }

        public static string ToString(object value)
        {
            if (value == null)
                return "null";

            if (value == DBNull.Value)
                return "DBNull";

            Type type = value.GetType();

            if (type.IsNullable() && value == null)
                return "null";

            if (type.Equals(typeof(string)))
            {
                if (((string)value).Length == 0)
                    return "String.Empty";

                return "\"" + (string)value + "\"";
            }
            else if (type.IsEnum)
			{
                return type.Name + "." + value.ToString();
			}
            else if (type.Equals(typeof(Boolean)))
            {
                return value.ToString().ToLower();
            }
            else if (type.Equals(typeof(Char)))
			{
                return "'" + value.ToString() + "'";
			}
            else if (type.Equals(typeof(byte[])))
            {
                return "byte[" + (value as byte[])!.Length + "]";
            }

            return Convert.ToString(value);
        }

        public static T ToObject<T>(string value)
		{
            throw new NotImplementedException();
            // TODO:
		}
    }
}
