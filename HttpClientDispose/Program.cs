using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientDispose
{
	class Program
	{
		static void Main(string[] args)
		{
			CallMethod("LogMessageWithDisposeProblem", LogMessageWithDisposeProblem);

			Console.WriteLine("================= Press Enter to continue =================");
			Console.ReadLine();

			CallMethod("LogMessageWithTask", LogMessageWithTask);

			Console.WriteLine("================= Press Enter to continue =================");
			Console.ReadLine();

			CallMethod("LogMessageAsyncAwait", LogMessageAsyncAwait);

			Console.ReadLine();
		}

		delegate Task Method(string message);

		private static void CallMethod(string methodName, Method methodDelegate)
		{
			try
			{
				WriteLog("CallMethod", "Calling " + methodName);
				methodDelegate("message");
				WriteLog("CallMethod", "Continuing after " + methodName);
			}
			catch (Exception e)
			{
				WriteLog("CallLogMessage", e.ToString());
			}
		}

		/// <summary>
		/// Original implementation with the Dispose problem.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		static Task LogMessageWithDisposeProblem(string message)
		{
			Uri baseAddress = new Uri("http://localhost/");
			string requestUri = "uri";
			string methodName = "LogMessageWithDisposeProblem";

			using (HttpClient client = new HttpClient { BaseAddress = baseAddress })
			{
				WriteLog(methodName, "Sending message");
				return client.PostAsJsonAsync(requestUri, message).ContinueWith(task =>
					{
						WriteLog(methodName, "Evaluating response");
						if (task.IsFaulted)
							Console.WriteLine("Failed: " + task.Exception);
						else if (task.IsCanceled)
							Console.WriteLine("Canceled");
						else
						{
							HttpResponseMessage response = task.Result;
							if (response.IsSuccessStatusCode)
								WriteLog(methodName, "Succeeded");
							else
								WriteLog(methodName, "Failed with status " + response.StatusCode);
						}
					});
			}
		}

		/// <summary>
		/// Improved implementation using an own task encapsulating the HttpClient.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		static Task LogMessageWithTask(string message)
		{
			Uri baseAddress = new Uri("http://localhost/");
			string requestUri = "uri";
			string methodName = "LogMessageWithTask";

			return new TaskFactory().StartNew(() =>
			{
				using (HttpClient client = new HttpClient { BaseAddress = baseAddress })
				{
					WriteLog(methodName, "Sending message");
					HttpResponseMessage response = client.PostAsJsonAsync(requestUri, message).Result;

					WriteLog(methodName, "Evaluating response");
					if (response.IsSuccessStatusCode)
						WriteLog(methodName, "Succeeded");
					else
						WriteLog(methodName, "Failed with status " + response.StatusCode);
				}
			});
		}

		/// <summary>
		/// Implementation using the async await pattern of .net 4.5.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		static async Task LogMessageAsyncAwait(string message)
		{
			Uri baseAddress = new Uri("http://localhost/");
			string requestUri = "uri";
			string methodName = "LogMessageAsyncAwait";

			using (HttpClient client = new HttpClient { BaseAddress = baseAddress })
			{
				WriteLog(methodName, "Sending message");
				HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, message);
				WriteLog(methodName, "Evaluating response");
				if (response.IsSuccessStatusCode)
					WriteLog(methodName, "Succeeded");
				else
					WriteLog(methodName, "Failed with status " + response.StatusCode);
			}
		}

		static void WriteLog(string method, string message)
		{
			Console.WriteLine("{0:HH:mm:ss.fff}\t{1}\t{2}\t{3}",
				DateTime.Now, Thread.CurrentThread.ManagedThreadId, method, message);
		}

	}
}
