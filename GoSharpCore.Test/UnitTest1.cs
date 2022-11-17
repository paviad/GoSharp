using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GoSharpCore.Test;

public class UnitTest1 {
    [Fact]
    public void SgfParseTest() {
        var sampleGames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var numTests = sampleGames.Count();
        var curTest = 0;
        Trace.WriteLine("Running " + numTests + " tests");
        foreach (var sourceFile in sampleGames) {
            curTest++;
            Trace.WriteLine("Running test #" + curTest + " on file " + sourceFile);
            string sourceText;
            using (var stream = Assembly.GetExecutingAssembly()
                       .GetManifestResourceStream(sourceFile))
            using (var reader = new StreamReader(stream)) {
                sourceText = reader.ReadToEnd();
            }

            File.WriteAllText("test.sgf", sourceText);

            var target = Game.SerializeFromSGF("test.sgf")[0];
            var gi = target.GameInfo;
            var expected = sourceText.Replace("\n", "");
            string actual;
            actual = target.SerializeToSGF(null);
            actual = Regex.Replace(actual, @"\s", "");
            actual = Regex.Replace(actual, @"A[BWE](\[[^]]+\])+", "");
            expected = Regex.Replace(expected, @"\s", "");
            expected = Regex.Replace(expected, @"A[BWE](\[[^]]+\])+", "");
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Test1() {
        var gi = new GameInfo {
            Handicap = 0,
            StartingPlayer = Content.Black,
            Komi = 6.5,
        };
        var gameInitial = new Game(gi);
        Game game;
        bool legal;

        game = gameInitial;
        var result = gameInitial.SerializeToSGF(null); // (;KM[6.50]SZ[19])

        game = game.MakeMove(3, 3, out legal);
        var result2 = gameInitial.SerializeToSGF(null); // (;KM[6.50]SZ[19];B[dd])

        game = game.MakeMove(16, 16, out legal);
        var result3 = gameInitial.SerializeToSGF(null); // (;KM[6.50]SZ[19];B[dd];W[qq])
    }

    [Fact]
    public void Test2() {
        var sgf = "(;GM[1]FF[4]AB[as](;B[dp])(;B[dq]))";
        var g = Game.SerializeFromSGF(new StringReader(sgf));

        var point_as = Point.ConvertFromSGF("as");
        var point_dp = Point.ConvertFromSGF("dp");
        var point_dq = Point.ConvertFromSGF("dq");

        Func<Game, Content> pieceAt_as = g => g.Board[point_as];
        Func<Game, Content> pieceAt_dp = g => g.Board[point_dp];
        Func<Game, Content> pieceAt_dq = g => g.Board[point_dq];

        var moves = g[0].Moves.ToList();
        var baseBoard = g[0];
        var variation1 = moves[0];
        var variation2 = moves[1];

        Assert.Equal(Content.Black, pieceAt_as(baseBoard));
        Assert.Equal(Content.Empty, pieceAt_dp(baseBoard));
        Assert.Equal(Content.Empty, pieceAt_dq(baseBoard));

        Assert.Equal(Content.Black, pieceAt_as(variation1));
        Assert.Equal(Content.Black, pieceAt_dp(variation1));
        Assert.Equal(Content.Empty, pieceAt_dq(variation1));

        Assert.Equal(Content.Black, pieceAt_as(variation2));
        Assert.Equal(Content.Empty, pieceAt_dp(variation2));
        Assert.Equal(Content.Black, pieceAt_dq(variation2));
    }
}
