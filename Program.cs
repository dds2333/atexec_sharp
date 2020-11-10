using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using CommandLine;
using Microsoft.Win32.TaskScheduler;

namespace atexec_sharp
{
	internal static class Program
	{
		static Program()
		{
			//Load DLL
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				string dllName = new AssemblyName(args.Name).Name + ".dll";
				var assm = Assembly.GetExecutingAssembly();
				var resourceName = assm.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName));
				if (resourceName == null)
				{
					return null;
				}

				using (var stream = assm.GetManifestResourceStream(resourceName))
				{
					byte[] assemblyData = new byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}
			};
		}

		private static void Main(string[] args)
		{
			Options options = new Options();
			Parser.Default.ParseArguments(args, options);
			bool is_arg = args.Length != 0;
			if (is_arg)
			{
				bool is_arg_2 = options.Host == null || options.Username == null || options.Password == null || options.Command == null;
				if (is_arg_2)
				{
					Console.WriteLine("Ex: atexec.exe -h ip -u username -p password -c command");
					Console.WriteLine("Ex: atexec.exe -h ip -u username -p password -d domain -c command");
					Console.WriteLine("Ex: atexec.exe -h ip -u username -p password -d domain -c command -s 5000");
				}
				else
				{
                    Task(options.Host, options.Username, options.Domain, options.Password, options.Command, options.CommandDelay);
				}
			}
			else
			{
				Console.WriteLine("[!] You need to specify a command with -c or --command see --help");
			}
		}

		public static void Task(string Host, string Username, string Domain, string Password, string Command, int CommandDelay)
		{
			Console.WriteLine("[!] This will work ONLY on Windows >= Vista");
			string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			Random random = new Random();
			string text2 = "";
			for (int i = 0; i < 6; i++)
			{
				text2 += text[random.Next(52)].ToString();
			}
			try
			{
				using (TaskService taskService = new TaskService(Host, Username, Domain, Password))
				{
					TaskDefinition taskDefinition = taskService.NewTask();
					taskDefinition.RegistrationInfo.Author = "system";
					taskDefinition.Principal.RunLevel = (TaskRunLevel)1;
					taskDefinition.Principal.UserId = "NT AUTHORITY\\System";
					taskDefinition.Triggers.Add(new DailyTrigger
					{
						DaysInterval = 1
					});
					ExecAction execAction = new ExecAction("c:\\windows\\system32\\cmd.exe", string.Concat(new string[]
					{
						"/c ",
						Command,
						" >C:\\windows\\temp\\",
						text2,
						".tmp"
					}), null);
					taskDefinition.Actions.Add(execAction);
					taskService.RootFolder.RegisterTaskDefinition(text2, taskDefinition);
					Console.Write("[*] Creating task \\" + text2 + "\r\n");
                    Task task = taskService.GetTask(text2);
					bool flag = task != null;
					if (flag)
					{
						task.Run(new string[0]);
						Console.Write("[*] Running task \\" + text2 + "\r\n");
						taskService.RootFolder.DeleteTask(text2);
						Console.Write("[*] Deleting task \\" + text2 + "\r\n");
						Thread.Sleep(CommandDelay);
						try
						{
							Cmd(text2, Host, Username, Password, Domain);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
					else
					{
						Console.Write("Task not found");
					}
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}

		public static void Cmd(string result, string Host, string Username, string Password, string Domain)
		{
			using (Runspace runspace = RunspaceFactory.CreateRunspace())
			{
				try
				{
					string text = string.Concat(new string[]
					{
						"net use \\\\",
						Host,
						" ",
						Password,
						" /user:",
						Domain,
						"\\",
						Username,
						";Get-Content \\\\",
						Host,
						"\\c$\\windows\\temp\\",
						result,
						".tmp;del \\\\",
						Host,
						"\\c$\\windows\\temp\\",
						result,
						".tmp;net use \\\\",
						Host,
						" /del"
					});
					runspace.Open();
					Pipeline pipeline = runspace.CreatePipeline();
					pipeline.Commands.AddScript(text);
					Collection<PSObject> collection = pipeline.Invoke();
					foreach (PSObject psobject in collection)
					{
						Console.WriteLine(psobject.ToString());
					}
				}
				catch
				{
				}
			}
		}
	}
}
