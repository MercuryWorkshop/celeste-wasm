namespace SimplexNoise
{
	public class Noise
	{
		public static byte[] perm = new byte[512]
		{
			151, 160, 137, 91, 90, 15, 131, 13, 201, 95,
			96, 53, 194, 233, 7, 225, 140, 36, 103, 30,
			69, 142, 8, 99, 37, 240, 21, 10, 23, 190,
			6, 148, 247, 120, 234, 75, 0, 26, 197, 62,
			94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
			33, 88, 237, 149, 56, 87, 174, 20, 125, 136,
			171, 168, 68, 175, 74, 165, 71, 134, 139, 48,
			27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
			60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
			245, 40, 244, 102, 143, 54, 65, 25, 63, 161,
			1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
			18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
			164, 100, 109, 198, 173, 186, 3, 64, 52, 217,
			226, 250, 124, 123, 5, 202, 38, 147, 118, 126,
			255, 82, 85, 212, 207, 206, 59, 227, 47, 16,
			58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
			119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
			101, 155, 167, 43, 172, 9, 129, 22, 39, 253,
			19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
			112, 104, 218, 246, 97, 228, 251, 34, 242, 193,
			238, 210, 144, 12, 191, 179, 162, 241, 81, 51,
			145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
			181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
			50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
			222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
			78, 66, 215, 61, 156, 180, 151, 160, 137, 91,
			90, 15, 131, 13, 201, 95, 96, 53, 194, 233,
			7, 225, 140, 36, 103, 30, 69, 142, 8, 99,
			37, 240, 21, 10, 23, 190, 6, 148, 247, 120,
			234, 75, 0, 26, 197, 62, 94, 252, 219, 203,
			117, 35, 11, 32, 57, 177, 33, 88, 237, 149,
			56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
			74, 165, 71, 134, 139, 48, 27, 166, 77, 146,
			158, 231, 83, 111, 229, 122, 60, 211, 133, 230,
			220, 105, 92, 41, 55, 46, 245, 40, 244, 102,
			143, 54, 65, 25, 63, 161, 1, 216, 80, 73,
			209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
			135, 130, 116, 188, 159, 86, 164, 100, 109, 198,
			173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
			5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
			207, 206, 59, 227, 47, 16, 58, 17, 182, 189,
			28, 42, 223, 183, 170, 213, 119, 248, 152, 2,
			44, 154, 163, 70, 221, 153, 101, 155, 167, 43,
			172, 9, 129, 22, 39, 253, 19, 98, 108, 110,
			79, 113, 224, 232, 178, 185, 112, 104, 218, 246,
			97, 228, 251, 34, 242, 193, 238, 210, 144, 12,
			191, 179, 162, 241, 81, 51, 145, 235, 249, 14,
			239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
			184, 84, 204, 176, 115, 121, 50, 45, 127, 4,
			150, 254, 138, 236, 205, 93, 222, 114, 67, 29,
			24, 72, 243, 141, 128, 195, 78, 66, 215, 61,
			156, 180
		};

		public static float Generate(float x)
		{
			int i0 = FastFloor(x);
			int i1 = i0 + 1;
			float x2 = x - (float)i0;
			float x3 = x2 - 1f;
			float num = 1f - x2 * x2;
			float num2 = num * num;
			float n0 = num2 * num2 * grad(perm[i0 & 0xFF], x2);
			float num3 = 1f - x3 * x3;
			float num4 = num3 * num3;
			float n1 = num4 * num4 * grad(perm[i1 & 0xFF], x3);
			return 0.395f * (n0 + n1);
		}

		public static float Generate(float x, float y)
		{
			float s = (x + y) * 0.3660254f;
			float x5 = x + s;
			float ys = y + s;
			int num = FastFloor(x5);
			int j = FastFloor(ys);
			float t = (float)(num + j) * 0.21132487f;
			float X0 = (float)num - t;
			float Y0 = (float)j - t;
			float x2 = x - X0;
			float y2 = y - Y0;
			int i1;
			int j2;
			if (x2 > y2)
			{
				i1 = 1;
				j2 = 0;
			}
			else
			{
				i1 = 0;
				j2 = 1;
			}
			float x3 = x2 - (float)i1 + 0.21132487f;
			float y3 = y2 - (float)j2 + 0.21132487f;
			float x4 = x2 - 1f + 0.42264974f;
			float y4 = y2 - 1f + 0.42264974f;
			int ii = num % 256;
			int jj = j % 256;
			float t2 = 0.5f - x2 * x2 - y2 * y2;
			float n0;
			if (t2 < 0f)
			{
				n0 = 0f;
			}
			else
			{
				t2 *= t2;
				n0 = t2 * t2 * grad(perm[ii + perm[jj]], x2, y2);
			}
			float t3 = 0.5f - x3 * x3 - y3 * y3;
			float n1;
			if (t3 < 0f)
			{
				n1 = 0f;
			}
			else
			{
				t3 *= t3;
				n1 = t3 * t3 * grad(perm[ii + i1 + perm[jj + j2]], x3, y3);
			}
			float t4 = 0.5f - x4 * x4 - y4 * y4;
			float n2;
			if (t4 < 0f)
			{
				n2 = 0f;
			}
			else
			{
				t4 *= t4;
				n2 = t4 * t4 * grad(perm[ii + 1 + perm[jj + 1]], x4, y4);
			}
			return 40f * (n0 + n1 + n2);
		}

		public static float Generate(float x, float y, float z)
		{
			float s = (x + y + z) * (1f / 3f);
			float x6 = x + s;
			float ys = y + s;
			float zs = z + s;
			int num = FastFloor(x6);
			int j = FastFloor(ys);
			int k = FastFloor(zs);
			float t = (float)(num + j + k) * (1f / 6f);
			float X0 = (float)num - t;
			float Y0 = (float)j - t;
			float Z0 = (float)k - t;
			float x2 = x - X0;
			float y2 = y - Y0;
			float z2 = z - Z0;
			int i1;
			int j2;
			int k2;
			int i2;
			int j3;
			int k3;
			if (x2 >= y2)
			{
				if (y2 >= z2)
				{
					i1 = 1;
					j2 = 0;
					k2 = 0;
					i2 = 1;
					j3 = 1;
					k3 = 0;
				}
				else if (x2 >= z2)
				{
					i1 = 1;
					j2 = 0;
					k2 = 0;
					i2 = 1;
					j3 = 0;
					k3 = 1;
				}
				else
				{
					i1 = 0;
					j2 = 0;
					k2 = 1;
					i2 = 1;
					j3 = 0;
					k3 = 1;
				}
			}
			else if (y2 < z2)
			{
				i1 = 0;
				j2 = 0;
				k2 = 1;
				i2 = 0;
				j3 = 1;
				k3 = 1;
			}
			else if (x2 < z2)
			{
				i1 = 0;
				j2 = 1;
				k2 = 0;
				i2 = 0;
				j3 = 1;
				k3 = 1;
			}
			else
			{
				i1 = 0;
				j2 = 1;
				k2 = 0;
				i2 = 1;
				j3 = 1;
				k3 = 0;
			}
			float x3 = x2 - (float)i1 + 1f / 6f;
			float y3 = y2 - (float)j2 + 1f / 6f;
			float z3 = z2 - (float)k2 + 1f / 6f;
			float x4 = x2 - (float)i2 + 1f / 3f;
			float y4 = y2 - (float)j3 + 1f / 3f;
			float z4 = z2 - (float)k3 + 1f / 3f;
			float x5 = x2 - 1f + 0.5f;
			float y5 = y2 - 1f + 0.5f;
			float z5 = z2 - 1f + 0.5f;
			int ii = Mod(num, 256);
			int jj = Mod(j, 256);
			int kk = Mod(k, 256);
			float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
			float n0;
			if (t2 < 0f)
			{
				n0 = 0f;
			}
			else
			{
				t2 *= t2;
				n0 = t2 * t2 * grad(perm[ii + perm[jj + perm[kk]]], x2, y2, z2);
			}
			float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
			float n1;
			if (t3 < 0f)
			{
				n1 = 0f;
			}
			else
			{
				t3 *= t3;
				n1 = t3 * t3 * grad(perm[ii + i1 + perm[jj + j2 + perm[kk + k2]]], x3, y3, z3);
			}
			float t4 = 0.6f - x4 * x4 - y4 * y4 - z4 * z4;
			float n2;
			if (t4 < 0f)
			{
				n2 = 0f;
			}
			else
			{
				t4 *= t4;
				n2 = t4 * t4 * grad(perm[ii + i2 + perm[jj + j3 + perm[kk + k3]]], x4, y4, z4);
			}
			float t5 = 0.6f - x5 * x5 - y5 * y5 - z5 * z5;
			float n3;
			if (t5 < 0f)
			{
				n3 = 0f;
			}
			else
			{
				t5 *= t5;
				n3 = t5 * t5 * grad(perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]], x5, y5, z5);
			}
			return 32f * (n0 + n1 + n2 + n3);
		}

		private static int FastFloor(float x)
		{
			if (!(x > 0f))
			{
				return (int)x - 1;
			}
			return (int)x;
		}

		private static int Mod(int x, int m)
		{
			int a = x % m;
			if (a >= 0)
			{
				return a;
			}
			return a + m;
		}

		private static float grad(int hash, float x)
		{
			int h = hash & 0xF;
			float grad = 1f + (float)(h & 7);
			if (((uint)h & 8u) != 0)
			{
				grad = 0f - grad;
			}
			return grad * x;
		}

		private static float grad(int hash, float x, float y)
		{
			int h = hash & 7;
			float u = ((h < 4) ? x : y);
			float v = ((h < 4) ? y : x);
			return ((((uint)h & (true ? 1u : 0u)) != 0) ? (0f - u) : u) + ((((uint)h & 2u) != 0) ? (-2f * v) : (2f * v));
		}

		private static float grad(int hash, float x, float y, float z)
		{
			int h = hash & 0xF;
			float u = ((h < 8) ? x : y);
			float v = ((h < 4) ? y : ((h == 12 || h == 14) ? x : z));
			return ((((uint)h & (true ? 1u : 0u)) != 0) ? (0f - u) : u) + ((((uint)h & 2u) != 0) ? (0f - v) : v);
		}

		private static float grad(int hash, float x, float y, float z, float t)
		{
			int h = hash & 0x1F;
			float u = ((h < 24) ? x : y);
			float v = ((h < 16) ? y : z);
			float w = ((h < 8) ? z : t);
			return ((((uint)h & (true ? 1u : 0u)) != 0) ? (0f - u) : u) + ((((uint)h & 2u) != 0) ? (0f - v) : v) + ((((uint)h & 4u) != 0) ? (0f - w) : w);
		}
	}
}
