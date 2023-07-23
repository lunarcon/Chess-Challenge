using System;
using ChessChallenge.API;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    List<Move> takenMoves = new List<Move>();
    int moveCount = 0;
    public int isTakenOrReversed(Move move) {
        for (int i = 0; i < takenMoves.Count; i++) {
            if (takenMoves[i].Equals(move)) {
                return 1;
            }
            if (takenMoves[i].StartSquare.Equals(move.TargetSquare) && takenMoves[i].TargetSquare.Equals(move.StartSquare)) {
                return 2;
            }
        }
        return 0;
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        int maxScore = -1;
        int maxIndex = -1;

        Random rng = new Random();
        
        for (int i = 0; i < moves.Length; i++)
        {
            int promotion = moves[i].IsPromotion ? 1000 : 0;
            int enpassant = moves[i].IsEnPassant ? (int)(100*Math.Log(moveCount+2)) : 0; // hehehe
            int castles = moves[i].IsCastles ? (int)(500/Math.Log(moveCount+2)) : 0;
            int current = (int) board.GetPiece(moves[i].StartSquare).PieceType;
            int target = (int) board.GetPiece(moves[i].TargetSquare).PieceType;

            int cur = (int)(promotion + enpassant + castles - 
            isTakenOrReversed(moves[i])*rng.Next(250) - 
            current*50 + target*1000 + getPromotionValue(board, moves[i])*500 +
            isMate(moves[i], board)*10000) - getThreats(board, moves[i])*1000;
            
            if (cur > maxScore) {  maxScore = cur; maxIndex = i; }
        }
        moveCount++;
        if (maxIndex>0 && !moves[maxIndex].IsNull) {
            takenMoves.Add(moves[maxIndex]);
            if (takenMoves.Count > 3) {
                takenMoves.RemoveAt(0);
            }
            return moves[maxIndex];
        } 
        return moves[0];
    }

    public int isMate(Move move, Board board) {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate ? 1 : 0;
    }

    public int getPromotionValue(Board board, Move move) {
        if (move.IsPromotion) {
            board.MakeMove(move);
            int promotionValue = (int) board.GetPiece(move.TargetSquare).PieceType;
            board.UndoMove(move);
            return promotionValue + move.TargetSquare.Rank;
        }
        return 0;
    }

    public int getThreats(Board board, Move move) {
        board.MakeMove(move);
        Move[] moves = board.GetLegalMoves();
        int threats = 0;
        for (int i = 0; i < moves.Length; i++) {
            if (moves[i].TargetSquare.Equals(move.TargetSquare)) {
                threats++;
            }
        }
        board.UndoMove(move);
        return threats;
    }
}