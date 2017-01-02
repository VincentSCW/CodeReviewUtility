using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utility.Library.WeakEvents;
using System.Windows.Input;

namespace Utility.Library.Commands
{
	/// <summary>
	/// Interface that is used for ICommands that notify when they are
	/// completed
	/// </summary>
	public interface ICompletionAwareCommand
	{
		/// <summary>
		/// Notifies that the command has completed
		/// </summary>
		//event Action<Object> CommandCompleted;
		WeakActionEvent<object> Completed { get; set; }
	}


	/// <summary>
	/// Simple delegating command, based largely on DelegateCommand from PRISM/CAL
	/// </summary>
	/// <typeparam name="T">The type for the </typeparam>
	public class SimpleCommand<T1, T2> : ICommand, ICompletionAwareCommand
	{
		private Func<T1, bool> canExecuteMethod;
		private Action<T2> executeMethod;
        private bool asynOp;
		public WeakActionEvent<object> Completed { get; set; }


		public SimpleCommand(Func<T1, bool> canExecuteMethod, Action<T2> executeMethod, bool asynOp = false)
		{
			this.executeMethod = executeMethod;
			this.canExecuteMethod = canExecuteMethod;
            this.asynOp = asynOp;
			this.Completed = new WeakActionEvent<object>();
		}

		public SimpleCommand(Action<T2> executeMethod)
		{
			this.executeMethod = executeMethod;
			this.canExecuteMethod = (x) => { return true; };
			this.Completed = new WeakActionEvent<object>();
		}
		
		public bool CanExecute(T1 parameter)
		{
			if (canExecuteMethod == null) 
				return true;
			return canExecuteMethod(parameter);
		}

		public void Execute(T2 parameter)
		{
			if (executeMethod != null)
			{
				executeMethod(parameter);
			}

			//now raise CommandCompleted for this ICommand
            if (!this.asynOp)
			{
                this.FireCompleted(parameter);
			}
		}

		public bool CanExecute(object parameter)
		{
			return CanExecute((T1)parameter);
		}

		public void Execute(object parameter)
		{
			Execute((T2)parameter);
		}


        public void FireCompleted(T2 parameter)
        {
            if(this.Completed != null)
			{
				Completed.Invoke(parameter);
			}
        }

		/// <summary>
		/// Occurs when changes occur that affect whether the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (canExecuteMethod != null)
				{
					CommandManager.RequerySuggested += value;
				}
			}

			remove
			{
				if (canExecuteMethod != null)
				{
					CommandManager.RequerySuggested -= value;
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="CanExecuteChanged" /> event.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}
}
