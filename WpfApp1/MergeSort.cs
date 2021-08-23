using System;

namespace WpfApp1
{
    public static class MergeSort
    {
        public static double[] Sort(double[] arr)
        {
            int len = arr.Length;
            if (len < 2)
            {
                return arr;
            }
            int middle = (int)Math.Floor((decimal)(len / 2));
            double[] left = Slice(0, middle, arr);
            double[] right = Slice(middle, arr.Length, arr);
            return Merge(Sort(left), Sort(right));
        }

        private static double[] Merge(double[] left, double[] right)
        {
            double[] result = new double[left.Length + right.Length];
            int index = 0;
            int indexL = 0;
            int indexR = 0;
            while (left.Length > indexL && right.Length > indexR)
            {
                if (left[indexL] <= right[indexR])
                {
                    result[index++] = left[indexL++];
                }
                else
                {
                    result[index++] = right[indexR++];
                }
            }

            while (left.Length > indexL)//多出来的部分
                result[index++] = left[indexL++];

            while (right.Length > indexR)
                result[index++] = right[indexR++];

            return result;
        }
        public static double[] Slice(int start, int end, double[] arr)
        {
            int index = start;
            double[] result = new double[end - start];
            for (int i = 0; i < end - start; i++)
            {
                result[i] = arr[index++];
            }
            return result;
        }
    }
}
