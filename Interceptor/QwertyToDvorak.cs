using Interceptor;
using System.Collections.Generic;

namespace DvorakKeyboard
{
	public static class QwertyToDvorak
	{
		private readonly static Dictionary<Keys, Keys> mapping = new Dictionary<Keys, Keys>()
		{
			[Keys.DashUnderscore] = Keys.OpenBracketBrace,
			[Keys.PlusEquals] = Keys.CloseBracketBrace,
			[Keys.Q] = Keys.SingleDoubleQuote,
			[Keys.W] = Keys.CommaLeftArrow,
			[Keys.E] = Keys.PeriodRightArrow,
			[Keys.R] = Keys.P,
			[Keys.T] = Keys.Y,
			[Keys.Y] = Keys.F,
			[Keys.U] = Keys.G,
			[Keys.I] = Keys.C,
			[Keys.O] = Keys.R,
			[Keys.P] = Keys.L,
			[Keys.OpenBracketBrace] = Keys.ForwardSlashQuestionMark,
			[Keys.CloseBracketBrace] = Keys.PlusEquals,
			//    [Keys.BACK_SLASH] = Keys.BACK_SLASH,
			//    [Keys.A] = Keys.A,
			[Keys.S] = Keys.O,
			[Keys.D] = Keys.E,
			[Keys.F] = Keys.U,
			[Keys.G] = Keys.I,
			[Keys.H] = Keys.D,
			[Keys.J] = Keys.H,
			[Keys.K] = Keys.T,
			[Keys.L] = Keys.N,
			[Keys.SemicolonColon] = Keys.S,
			[Keys.SingleDoubleQuote] = Keys.DashUnderscore,
			[Keys.Z] = Keys.SemicolonColon,
			[Keys.X] = Keys.Q,
			[Keys.C] = Keys.J,
			[Keys.V] = Keys.K,
			[Keys.B] = Keys.X,
			[Keys.N] = Keys.B,
			//    [Keys.M] = Keys.M,
			[Keys.CommaLeftArrow] = Keys.W,
			[Keys.PeriodRightArrow] = Keys.V,
			[Keys.ForwardSlashQuestionMark] = Keys.Z,
		};

		public static Keys MapKey(Keys key)
		{
			if(mapping.ContainsKey(key))
			{
				return mapping[key];
			}

			return key;
		}
	}
}