using System;

namespace com.unity.mgobe.src.Util {
    public static class RandomUtil {
        private static double _xn1 = 0;
        private static double _a = 1103515245;
        private static double _b = 123456789;
        private static double _m = Math.Pow (2, 32) - 1;

        /**
         * @doc RandomUtil.init
         * @name 初始化随机数
         * @description init 方法接受一个 seed 为参数，RandomUtil 在后续生成随机数的过程中将以 seed 为种子。使用相同的 seed 初始化，调用 random 方法生成的随机数序列相同。
         * @param {number} seed 随机数种子
         * @returns {void}
         */
        public static void Init (int seed) {
            _xn1 = seed;
        }

        /**
         * @doc RandomUtil.random
         * @name 生成随机数
         * @description 如果种子相同、初始化后调用次数相同，生成的随机数将相同。
         * @returns {number} 随机数
         */
        public static double Random () {
            var x = (_a * _xn1 + _b) % _m;
            _xn1 = x;

            return x / _m;
        }
    }

}