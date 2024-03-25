using System.Collections.Generic;

namespace Game.Match3.Model
{

    public interface IBoard
    {

        int Width { get; }
        int Height { get; }

        Piece CreatePiece(int pieceType, int x, int y);
        Piece GetAt(int x, int y);

        void MovePiece(int fromX, int fromY, int toX, int toY);
        void RemovePieceAt(int x, int y);

        List<Piece> GetConnected(int x, int y);
        bool TryGetPiecePos(Piece piece, out int px, out int py);

    }

}