using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace OpFlow.Service.DataAccess
{
    public class EntityReader<T> : IEnumerable<T>, IDisposable
            where T : new()
    {
        private DataTable mDT;
        private readonly Dictionary<string, int> mColumns;
        private readonly List<Action<object, DataRow>> mSetters;
        private readonly bool mAutoDisposeReader;

        public EntityReader(DataTable dt, bool autoDisposeReader = true)
        {
            mDT = dt;
            mAutoDisposeReader = autoDisposeReader;

            mColumns = dt.Columns.Cast<DataColumn>().ToDictionary(x => x.ColumnName, x => x.Ordinal, StringComparer.OrdinalIgnoreCase);
            mSetters = GetSetters(typeof(T));
        }

        private List<Action<object, DataRow>> GetSetters(Type t)
        {
            lock (EntityReaderHelper.CachedSetMethodsLock)
            {
                var setters = new List<Action<object, DataRow>>();
                
                var props =
                    t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty |
                                    BindingFlags.GetProperty);
                props = props.Where(x =>
                {
                    var setter = x.GetSetMethod();
                    return x.GetIndexParameters().Length == 0 && setter != null && setter.IsPublic;
                }).ToArray();

                foreach (var propInfo in props)
                {
                    int column;
                    var prop = propInfo;
                    if (mColumns.TryGetValue(prop.Name, out column))
                    {
                        var valueSetter = EntityReaderHelper.GetValueSetter(t, prop);
                        setters.Add((o, dr) =>
                        {
                            object val = dr.ItemArray[column];
                            if (DBNull.Value.Equals(val) || val == null)
                            {
                                try
                                {
                                    if (prop.PropertyType.IsValueType)
                                    {
                                        valueSetter(o, Activator.CreateInstance(prop.PropertyType));
                                    }
                                    else
                                    {
                                        valueSetter(o, null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var colName = mColumns.Where(x => x.Value == column).Select(x => x.Key).FirstOrDefault();

                                    var exText = string.Format("Error setting {0}.{1} to '{2}' ({3})",
                                        o.GetType().FullName, colName ?? "unknown", val,
                                        val == null ? "null" : val.GetType().FullName);

                                    if (EntityReaderHelper.ThrowExceptionsForInexactPocoDataBindings)
                                    {
                                        throw new Exception(exText, ex);
                                    }

                                    EntityReaderHelper.LogException(new Exception(exText, ex));

                                    // Change the value setter to a slower, but more flexible method, 
                                    valueSetter = EntityReaderHelper.FallbackValueSetter(t, prop);
                                    // then try again.
                                    valueSetter(o, null);
                                }
                            }
                            else
                            {
                                if (prop.PropertyType == typeof(Char) && val is String)
                                {
                                    var s = val as string;
                                    val = s.Length > 0 ? s[0] : default(Char);
                                }
                                else if ((prop.PropertyType == typeof(DateTimeOffset) || prop.PropertyType == typeof(DateTimeOffset?)) && val is DateTimeOffset)
                                {
                                    val = (DateTimeOffset)val;

                                    if (prop.GetCustomAttributes(false).OfType<DateOnlyAttribute>().Any())
                                    {
                                        val = new DateTimeOffset(((DateTimeOffset)val).Date);
                                    }
                                }
                                else if ((prop.PropertyType == typeof(DateTimeOffset) || prop.PropertyType == typeof(DateTimeOffset?)) && val is DateTime)
                                {
                                    val = (DateTimeOffset)(DateTime)val;

                                    if (prop.GetCustomAttributes(false).OfType<DateOnlyAttribute>().Any())
                                    {
                                        val = new DateTimeOffset(((DateTimeOffset)val).Date);
                                    }
                                }
                                else if ((prop.PropertyType == typeof(DateTimeOffset) || prop.PropertyType == typeof(DateTimeOffset?)) && val is String)
                                {
                                    val = DateTimeOffset.Parse((string)val);

                                    if (prop.GetCustomAttributes(false).OfType<DateOnlyAttribute>().Any())
                                    {
                                        val = new DateTimeOffset(((DateTimeOffset)val).Date);
                                    }
                                }
                                else if ((prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?)) && val is decimal)
                                {
                                    val = (int)(decimal)val;
                                }
                                else if (prop.PropertyType.BaseType.IsGenericType && prop.PropertyType.BaseType.Name.StartsWith("ValAsEnum`"))
                                {
                                    var valProp = prop.PropertyType.GetField("Val", BindingFlags.NonPublic | BindingFlags.Instance);
                                    val = prop.PropertyType.GetFields(BindingFlags.Public | BindingFlags.Static)
                                        .Select(x => x.GetValue(null))
                                        .FirstOrDefault(x =>
                                                    ((IComparable)valProp.GetValue(x)).CompareTo(val) == 0
                                        );
                                }
                                else if (prop.PropertyType.IsEnum)
                                    val = Enum.ToObject(prop.PropertyType, val);
                                else if (prop.PropertyType.IsGenericType && prop.PropertyType.Name.StartsWith("Nullable`") && prop.PropertyType.GenericTypeArguments[0].IsEnum)
                                    val = Enum.ToObject(prop.PropertyType.GenericTypeArguments[0], val);
                                else if ((prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)) && (val is long))
                                    val = (decimal)(long)val;
                                else if ((prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)) && (val is int))
                                    val = (decimal)(int)val;
                                else if ((prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)) && (val is double))
                                    val = (decimal)(double)val;
                                else if (prop.PropertyType.GetInterface(typeof(ICastable).FullName) != null)
                                {
                                    var dataParam = Expression.Parameter(typeof(object), "data");
                                    var body = Expression.Block(Expression.Convert(Expression.Convert(dataParam, val.GetType()), prop.PropertyType));

                                    var run = Expression.Lambda(body, dataParam).Compile();
                                    val = run.DynamicInvoke(val);
                                }
                                else if ((prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?)) && (val is short))
                                    val = (int)(short)val;
                                else if ((prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?)) && (val is string))
                                    val = int.Parse((string)val);
                                else if ((prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)) && (val is string))
                                    val = decimal.Parse((string)val);

                                try
                                {
                                    valueSetter(o, val);
                                }
                                catch (Exception ex)
                                {
                                    var colName = mColumns.Where(x => x.Value == column).Select(x => x.Key).FirstOrDefault();
                                    var exText = string.Format("Error setting {0}.{1} to '{2}' ({3})",
                                        o.GetType().FullName, colName, val,
                                        val == null ? "null" : val.GetType().FullName);

                                    if (EntityReaderHelper.ThrowExceptionsForInexactPocoDataBindings)
                                    {
                                        //GeneratePocoBindingForQueryResult();

                                        throw new Exception(exText, ex);
                                    }

                                    EntityReaderHelper.LogException(new Exception(exText, ex));

                                    // Change the value setter to a slower, but more flexible method, 
                                    valueSetter = EntityReaderHelper.FallbackValueSetter(t, prop);
                                    // then try again.
                                    valueSetter(o, val);
                                }
                            }
                        });
                    }
                }

                return setters;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EntityEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (mDT != null && mAutoDisposeReader)
            {
                mDT.Dispose();
                mDT = null;
            }
        }

        private void Populate(T obj, DataRow row)
        {
            foreach (var setter in mSetters)
            {
                setter(obj, row);
            }
        }

        private class EntityEnumerator : IEnumerator<T>
        {
            private EntityReader<T> mOwner;
            private T mCurrent;
            private IEnumerator mRowEnumerator;

            public EntityEnumerator(EntityReader<T> owner)
            {
                mOwner = owner;
                mRowEnumerator = mOwner.mDT.Rows.GetEnumerator();
            }

            public void Dispose()
            {
                if (mOwner != null)
                {
                    mOwner.Dispose();
                    mOwner = null;
                }
            }

            public bool MoveNext()
            {
                mCurrent = default(T);
                return mRowEnumerator.MoveNext();
            }

            public void Reset()
            {
                mRowEnumerator.Reset();
            }

            public T Current
            {
                get
                {
                    if (mCurrent == null)
                    {
                        mCurrent = new T();

                        mOwner.Populate(mCurrent, mRowEnumerator.Current as DataRow);
                    }
                    return mCurrent;
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        //private void GeneratePocoBindingForQueryResult()
        //{
        //    Debug.WriteLine("-------- Suggested POCO Bindings for failed POCO translation --------");
        //    var i = 0;
        //    foreach (DataRow col in mSchemaTable.Rows)
        //    {
        //        Debug.WriteLine("[TableValueParameterOrder(" + (++i) + ")]");
        //        Debug.Write("public ");

        //        var type = (Type)col["DataType"];
        //        var isNullable = (bool)col["AllowDBNull"];
        //        var columnName = (string)col["ColumnName"];

        //        if (type == typeof(string))
        //        {
        //            Debug.Write("string");
        //            isNullable = false; // Don't want the question mark for a string type.
        //        }
        //        else if (type == typeof(Int32))
        //        {
        //            Debug.Write("int");
        //        }
        //        else if (type == typeof(Int64))
        //        {
        //            Debug.Write("long");
        //        }
        //        else if (type == typeof(Decimal))
        //        {
        //            Debug.Write("decimal");
        //        }
        //        else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        //        {
        //            Debug.Write("DateTimeOffset");
        //        }
        //        else if (type == typeof(Guid))
        //        {
        //            Debug.Write("Guid");
        //        }
        //        else if (type == typeof(bool))
        //        {
        //            Debug.Write("bool");
        //        }
        //        else
        //        {
        //            Debug.Write("{ " + type.Name + " }");
        //        }

        //        if (isNullable)
        //            Debug.Write("?");

        //        Debug.WriteLine(" " + columnName + " { get; set; }");
        //    }
        //    Debug.WriteLine("---------------------------------------------------------------------");
        //}
    }

    public static class EntityReaderHelper
    {
        public static bool ThrowExceptionsForInexactPocoDataBindings = GetThrowExceptionsForInexactPocoDataBindingsConfigValue();

        private static bool GetThrowExceptionsForInexactPocoDataBindingsConfigValue()
        {
            //var setting = ConfigurationManager.AppSettings["ThrowExceptionsForInexactPocoDataBindings"];
            //bool result;
            //if (bool.TryParse(setting, out result))
            //{
            //    return result;
            //}
            return true; // Default to true;
        }

        public delegate void ValueSetter(object target, object value);

        public static readonly object CachedSetMethodsLock = new object();

        /// <summary>
        /// The outer dictionary key is the full type name, the inner dictionary key is the property name.
        /// </summary>
        public static readonly Dictionary<string, Dictionary<string, ValueSetter>> CachedSetMethods =
            new Dictionary<string, Dictionary<string, ValueSetter>>();


        public static ValueSetter GetValueSetter(Type type, PropertyInfo propInfo)
        {
            ValueSetter result;
            Dictionary<string, ValueSetter> propCache;
            if (!CachedSetMethods.TryGetValue(type.FullName, out propCache))
            {
                Debug.WriteLine(string.Format("Caching value setters for type: {0}", type.FullName));

                propCache = new Dictionary<string, ValueSetter>();
                CachedSetMethods[type.FullName] = propCache;
            }
            if (propCache.TryGetValue(propInfo.Name, out result))
            {
                return result;
            }

            var method = propInfo.GetSetMethod();
            if (method == null)
            {
                return null;
            }

            var objType = typeof(object);
            var setter = new DynamicMethod("x", null, new[] { objType, objType });
            var generator = setter.GetILGenerator();

            if (!type.IsClass) // Used for structs
            {
                var local = generator.DeclareLocal(type);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Unbox_Any, type);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldloca_S, local);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(propInfo.PropertyType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any,
                    propInfo.PropertyType);
                generator.EmitCall(OpCodes.Call, method, null);
            }
            else
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, propInfo.DeclaringType);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(propInfo.PropertyType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any,
                    propInfo.PropertyType);
                generator.EmitCall(OpCodes.Callvirt, method, null);
            }

            generator.Emit(OpCodes.Ret);

            result = (ValueSetter)setter.CreateDelegate(typeof(ValueSetter));

            propCache[propInfo.Name] = result;
            return result;
        }

        public static void CacheValueSetters(Type t)
        {
            lock (CachedSetMethodsLock)
            {
                var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty |
                                            BindingFlags.GetProperty);
                props = props.Where(
                    x =>
                    {
                        var setter = x.GetSetMethod();
                        return x.GetIndexParameters().Length == 0 && setter != null && setter.IsPublic;
                    }
                    ).ToArray();

                foreach (var propInfo in props)
                {
                    GetValueSetter(t, propInfo);
                }
            }
        }

        public static ValueSetter FallbackValueSetter(Type type, PropertyInfo propInfo)
        {
            var propCache = CachedSetMethods[type.FullName];
            var result = (ValueSetter)((target, value) => propInfo.SetValue(target, value, null));
            propCache[propInfo.Name] = result;
            return result;
        }

        public static void LogException(Exception exception)
        {
        }
    }

    public interface ICastable
    {
        bool CanCastFrom(Type type);
        object Cast(object val);
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DateOnlyAttribute : Attribute
    {

    }
}