﻿using BenchmarkDotNet.Running;
using CombatlogParser.Parsing;

namespace Benchmarks;

public class Program
{
	public static void Main()
	{
		BenchmarkRunner.Run<SubeventParse>();
	}
}

[SimpleJob]
public class SubeventParse
{
	private readonly string[] subevents =
	[
		"SPELL_DAMAGE",
		"SPELL_AURA_APPLIED",
		"SPELL_PERIODIC_DAMAGE_SUPPORT",
		"RANGE_DAMAGE",
		"SWING_DAMAGE_LANDED"
	];

	[Benchmark]
	public void TryParse()
	{
		foreach (var subevent in subevents)
		{
			_ = ParsingUtil.TryParsePrefixSuffixSubeventF(subevent, out _, out _);
		}
	}
}