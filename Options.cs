using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace atexec_sharp
{
	internal class Options
	{
		[Option('h', "host", Required = false, HelpText = "Host (IP address or hostname - default: localhost)")]
		public string Host { get; set; }

		[Option('u', "username", Required = false, HelpText = "Username to authenticate with")]
		public string Username { get; set; }

		[Option('p', "password", Required = false, HelpText = "Password to authenticate with")]
		public string Password { get; set; }

		[Option('d', "domain", Required = false, HelpText = "Domain to authenticate with")]
		public string Domain { get; set; }

		[Option('v', "Verbose", DefaultValue = false, HelpText = "Prints all messages to standard output.")]
		public bool Debug { get; set; }

		[Option('c', "Command", Required = false, DefaultValue = null, HelpText = "Command to run e.g. \"nestat-ano\" ")]
		public string Command { get; set; }

		[Option('s', "CommandSleep", Required = false, DefaultValue = 3000, HelpText = "Command sleep in milliseconds - increase if getting truncated output")]
		public int CommandDelay { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, delegate (HelpText current)
			{
				HelpText.DefaultParsingErrorsHandler(this, current);
			}, false);
		}
	}
}
