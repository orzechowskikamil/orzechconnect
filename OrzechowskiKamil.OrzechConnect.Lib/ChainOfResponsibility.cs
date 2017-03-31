using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace OrzechowskiKamil.OrzechConnect.Lib
{
	/// <summary>
	/// Moja byc moze nie do konca poprawna implementacja wzorca chain of responsibility
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ChainOfResponsibility<T>
	{
		public T StaticMethods { get; set; }
		private List<Action<ChainOfResponsibility<T>>> actions;
		private int currentMethodIndex;

		public ChainOfResponsibility( T staticMethods )
			: this()
		{
			this.StaticMethods = staticMethods;

		}

		public ChainOfResponsibility()
		{
			this.currentMethodIndex = -1;
			this.actions = new List<Action<ChainOfResponsibility<T>>>();
		}

		public ChainOfResponsibility<T> Add( Action<ChainOfResponsibility<T>> action )
		{
			this.actions.Add( action );
			return this;
		}

		public void CallNext()
		{
			if (this.actions.Count > this.currentMethodIndex && this.AbortExecution == false)
			{
				var shouldCallNext = true;
				if (this.BeforeCallNext != null)
				{
					shouldCallNext = this.BeforeCallNext( this );
				}
				if (this.AbortExecution)
				{
					shouldCallNext = false;
				}
				if (shouldCallNext)
				{
					var action = this.actions[this.currentMethodIndex];
					try
					{
						if (this.NotStandardCallNext != null)
						{
							this.NotStandardCallNext( this, action );
						}
						else
						{
							this.currentMethodIndex++;
							action( this );
						}
					}
					catch (Exception exception)
					{
						if (this.OnExceptionOccured != null)
						{
							this.OnExceptionOccured( this,exception );
						}
					}

				}
				
				if (this.AutomaticExecution == true) { this.CallNext(); }
			}

		}

		public ChainOfResponsibility<T> Start()
		{
			this.currentMethodIndex = 0;
			this.CallNext();
			return this;
		}

		/// <summary>
		/// Ustaw na true aby przerwac wykonywanie automatyczne łańcucha
		/// </summary>
		public bool AbortExecution { get; set; }
		/// <summary>
		/// Ustaw na true, aby łańcuch sam uruchamial swoje kolejne metody
		/// </summary>
		public bool AutomaticExecution { get; set; }

		/// <summary>
		/// Jezeli akcja zwroci false, wykonywanie metody zostanie przerwane (łańcuch sprobuje wykonac następną w łańcuchu)
		/// </summary>
		public Func<ChainOfResponsibility<T>, bool> BeforeCallNext;
		/// <summary>
		/// Jesli to zostanie ustawione ta funkcja sie wykona zamiast standardowego callnext.
		/// </summary>
		public Action<ChainOfResponsibility<T>, Action<ChainOfResponsibility<T>>> NotStandardCallNext;
		public Action<ChainOfResponsibility<T>, Exception> OnExceptionOccured;
	}

}
