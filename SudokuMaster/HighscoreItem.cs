/*
 * Copyright (c) 2011 Nokia Corporation.
 */

using System;
using System.Xml;
using System.Xml.Serialization;

namespace Balda
{
	/// <summary>
	/// Represents a single score in highscore list.
	/// </summary>
	public class HighscoreItem
	{
		public int Index { get; set; }
		public string Name { get; set; }
		public int player1 { get; set; }
        public int player2 { get; set; }

		// XmlSerializer cannot serialize TimeSpans. Tell the serializer to
		// ignore Time, and (de)serialize the Time as string instead. 
		[XmlIgnore]
		public TimeSpan Time { get; set; }

		[XmlAttribute("TimeString", DataType = "duration")]
		public string XmlTime
		{
			get { return XmlConvert.ToString(Time); }
			set { Time = XmlConvert.ToTimeSpan(value); }
		}

		/// <summary>
		/// Constructor
		/// Serializable classes must have default constructor without any
		/// arguments. XmlSerializer knows the member variables of given class,
		/// but nothing about the constructor's arguments.
		/// </summary>
		public HighscoreItem()
		{
            Index = 0;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="scoreIndex">Position in highscore list</param>
		/// <param name="playerName">Player's name</param>
		/// <param name="solvingTime">Time needed to solve the puzzle</param>
		/// <param name="playerMoves">Moves needed to solve the puzzle</param>
		public HighscoreItem(TimeSpan solvingTime, int p1, int p2)
		{
            player1 = p1;
            player2 = p2;
			Time = solvingTime;
			
		}
	}
}
