using System;
using System.Collections.Generic;

namespace rbush.net
{
    public static class QuickSelect<T>
    {
        public static T Select(ref IList<T> list, int indexK, int left, int right, Comparison<T> comparer)
        {
            while (right > left)
            {
                if (right - left > 600)
                {
                    var n = right - left + 1;
                    var m = indexK - left + 1;
                    var z = Math.Log(n);
                    var s = 0.5 * Math.Exp(2 * z / 3);
                    var sd = 0.5 * Math.Sqrt(z * s * (n - s) / n) * (m - n / 2 < 0 ? -1 : 1);
                    var newLeft = (int)Math.Max(left, Math.Floor(indexK - m * s / n + sd));
                    var newRight = (int)Math.Min(right, Math.Floor(indexK + (n - m) * s / n + sd));
                    Select(ref list, indexK, newLeft, newRight, comparer);
                }

                var t = list[indexK];
                var i = left;
                var j = right;

                Swap(list, left, indexK);
                if (comparer(list[right], t) > 0) Swap(list, left, right);

                while (i < j)
                {
                    Swap(list, i, j);
                    i++;
                    j--;
                    while (comparer(list[i], t) < 0) i++;
                    while (comparer(list[j], t) > 0) j--;
                }

                if (comparer(list[left], t) == 0) Swap(list, left, j);
                else
                {
                    j++;
                    Swap(list, j, right);
                }

                if (j <= indexK) left = j + 1;
                if (indexK <= j) right = j - 1;
            }

            return list[left];
        }

        private static void Swap(IList<T> list, int i, int j)
        {
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
