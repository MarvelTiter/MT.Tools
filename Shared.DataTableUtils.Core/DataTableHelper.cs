using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Shared.DataTableUtils.Core {
    public static class DataTableHelper {
        public static bool HasRows(this DataTable dt) {
            return dt != null && dt.Rows.Count > 0;
        }

        public static IEnumerable<T> ToEnumerable<T>(this DataTable dt) {
            foreach (DataRow item in dt.Rows) {
                yield return item.Parse<T>();
            }
        }

        public static T Parse<T>(this DataRow row) {
            return default;
        }
    }
}
