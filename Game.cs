﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAI
{

    public struct Position : IEquatable<Position> //Define a struct named Position. A struct is a custom datatype. Inheriting IEquatable tells the code that instances of Position can be checked for equality with each other
    {
        public Position(byte row, byte column) //Structs have class-like constructor methods. A byte is just an int that only spans one byte in memory.
                                               //Since the positions on a chess board are small numbers, I can use them here, and they are a little faster and more memory efficient.
        {
            nrow = row; //I named the internal variables nrow and ncolumn because I had so many variables named row and column I couldn't do it with capitalization without being ugly, so I just did this instead.
            ncolumn = column;
        }
        private byte nrow; //The internal variables of the struct are private, since the values should be checked for validity before being used.
        private byte ncolumn;
        public byte Row //Define a property named Row.
        {
            get => nrow; //This => operator just tells it to define the function on the left to return the expression on the right. It's a lot like lambda in python.
            set //The set method of a property tells the code what to do when code tries to set the value of the property. Inside the set method, there is a special variable named value which is just whatever the property is being set to.
            {
                if ((0 <= value) && (value < 8)) //Check if the value is 0, 1, 2, 3, 4, 5, 6, or 7
                {
                    ncolumn = value; //Set the internal value
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Row", "Row must be greater than or equal to 0 and less than 8");
                    //Crash the program with an error message. Functions setting a position should be doing their own checks to make sure the value makes sense; clamping it could lead to weird, hard to track down bugs.
                    //It's best to just crash and let me know immediately what's going wrong.
                }
            }
        }
        public byte Column //Define property named Column. Identical to Row except that it's the columns.
        {
            get => ncolumn;
            set
            {
                if ((0 <= value) && (value < 8)) {
                    ncolumn = value; 
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Column", "Column must be greater than or equal to 0 and less than 8");
                }
            }
        }

        public override bool Equals(object obj) //This Equals method was auto-generated when I told it to inherit IEquatable, I don't fully understand what it's doing but I will explain to the best of my ability
        {
            return obj is Position position && Equals(position); //I think it is saying that if the other object is of type position, run the other Equals method to check if they are equal
        }

        public bool Equals(Position other) //This is the one that I defined, it takes in another Position and returns true if both the row and column of the other are equal to the row and column of this position.
        {
            return (Column == other.Column) && (Row == other.Row);
        }

        public override int GetHashCode() //This is part of IEquatable and was auto-generated, my undertstanding is it generates a unique integer for all possible Positions that can be used to check for equality
        {
            int hashCode = 240067226;
            hashCode = hashCode * -1521134295 + Row.GetHashCode();
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Position left, Position right) //This was autogenerated, but I understand it fully. It defines how the == operator behaves between two positons, returning the value of the Equals method
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right) //This is the same as the above except it's for != (not equals), and works the same way but inverts the output
        {
            return !(left == right);
        }

    }

    class Game //Define a class called Game. I am going to use this class to hold various information that may need to be accessed in multiple places but which does not necessarily belong anywhere else.
               //It will mostly hold constant values that I may want to give names to but that are actually just aliases for simple constants, but it may also contain the gamestate later, I am yet to decide
    {
        public const byte PAWN = 1, KNIGHT = 2, BISHOP = 3, ROOK = 4, QUEEN = 5, KING = 6, PASSANT = 7;
        public const bool WHITE = true, BLACK = false;
        /* 1: Pawn
         * 2: Knight
         * 3: Bishop
         * 4: Rook
         * 5: Queen
         * 6: King
         */



    }

    public abstract class Piece : IEquatable<Piece> //Defining a class as abstract tells the program that I intend to use this class to define general properties that many subclasses will inherit, but will not actually instantiate any instances of the base class
    {

        public Position position; //Declare that there will be a Position type variable named position, but don't define it
        public bool white; //Declare a bool (boolean, true/false) to keep track of whether a given piece is white
        public bool canCastle = false;
        public abstract byte type { get; } //Declare an integer named type. This will be set in each of the subclasses

        public byte Row //Define a property named Row. Properties are declared like variables, but defined sort of like functions with get and set methods that tell the code how to find the value.
                        //It's basically saying that I want to be able to treat this like a variable when I use it elsewhere, but run some code to actually determine the value
        {
            get => position.Row;
            set
            {
                if (value < 8) { position.Row = value; }
            }
        }
        public byte Column //Define a property named Column
        {
            get => position.Column;
            set
            {
                if (value < 8) { position.Column = value; }
            }
        }

        public byte RenderID 
        { 
            get
            {
                return (byte)(type + (8 * Convert.ToInt32(white)));
            }
        }

        public abstract List<Position> Moves(GameState board); //Define an abstract method named Moves(). An abstract method is a method declared in an abstract class but with no implementation, to indicate that all subclasses
                                            //must define an implementation of the method. In this case, Moves() generates the legal moves a piece can take, which must be defined for each piece but is also
                                            //different for each piece, so it is defined as an abstract method to indicate that I must define it for all pieces.

        public virtual List<Position> Threaten(GameState board) //Define a virtual method named Threaten(). A virtual method is like an abstract method, but it has a default implementation which can be overridden by subclasses
                                             //but doesn't need to be. In this case, most pieces simply threaten all of the positions which are legal moves, but pawns function differently, so I define
                                             //a virtual method for Threaten which defines it as simply returning Moves() for most pieces, but allows me to override it in Pawn where it works differently.
        {
            List<Position> output = new List<Position>();
            foreach (Position move in Moves(board)) {
                output.Append(move);
            }
            return output;
        }

        public virtual bool Equals(Piece other) {
            return (position == other.position) &&
                (white == other.white) &&
                (type == other.type);
        }

        public override int GetHashCode() {
            return ((position.Row * 8) + position.Column) + (RenderID * 64);
        }

        public virtual void Moved(Position move, GameState board) {
            position = move;
        }

        public virtual bool CanCapture(Piece other) {
            return white != other.white;
        }
    }

    class Pawn : Piece //Each of the piece types will be its own class which inherits from the abstract Piece class but with its own implementation of some things for the differences between pieces
    {
        public override byte type { get => Game.PAWN; }
        public bool hasMoved = false;
        //I'm using static variables to be able to use a name for the types of different pieces in various functions without needing to pass strings or class instances around, which is much less efficient and
        //easy in C# than in python. Instead, I've named some variables with integer values that are more efficient and easy to handle but they still have names so I don't need to remember them.

        public Pawn(Position startPosition, bool isWhite) //The constructor method for Pawn is called when a Pawn is created. It takes in a position (the initial position of the pawn) and whether the pawn is white
        {
            position = startPosition; //Setting the internal position variable to the value of the temporary startPosition variable
            white = isWhite; //Setting the internal isWhite variable to the value of the temporary white variable
            
        }

        public override List<Position> Moves(GameState board)
        {
            List<Position> output = new List<Position>();
            byte newRow;
            if (white) {
                newRow = (byte)(Row - 1);
            }
            else {
                newRow = (byte)(Row + 1);
            }
            //Calculate the new row

            

            int farLeft = Column - 1;
            int farRight = Column + 1;
            if (farLeft < 0) { farLeft = 0; }
            if (farRight > 7) { farRight = 7; }

            for (int column = farLeft; column <= farRight; column++ ) {
                if ((white && Row > 0) || (!white && Row < 7)) {
                    output.Add(new Position(newRow, (byte)column));
                }
            }
            //Add all possible positions
            if (!hasMoved) { 
                output.Add(new Position((byte)(Row + (newRow - Row) * 2), Column));
            }

            List<Position> final = new List<Position>();
            final.AddRange(output);
            foreach (Position move in output) {
                Piece piece;
                if ((move.Column == Column) == (board.state.ContainsKey(move))) {
                    final.Remove(move);
                }
                else if ((move.Column != Column) && (board.state.TryGetValue(move, out piece))) {
                    if (piece.white == white) {
                        final.Remove(move);
                    }
                }
                
            }
            //Filter where the pawn is blocked in front or doesn't have a target diagonally


            return final;
            //This needs to be implemented later, but for now it just crashes the program
        }

        public override List<Position> Threaten(GameState board)
        {
            List<Position> output = new List<Position>();
            //Initialize the existance of output but don't define its values yet
            byte newRow = (byte)(Row + 1);
            if (white) { newRow = (byte)(Row - 1); }
            //Figure out the row

            switch (Column) {
                default:
                    output.Add(new Position(newRow, (byte)(Column + 1)));
                    output.Add(new Position(newRow, (byte)(Column - 1)));
                    break;
                case 0:
                    output.Add(new Position(newRow, (byte)(Column + 1)));
                    break;
                case 7:
                    output.Add(new Position(newRow, (byte)(Column - 1)));
                    break;
            }
            //Add the columns

            return output;
            
            //Return the result
        }

        public override int GetHashCode() {
            return ((position.Row * 8) + position.Column) + (RenderID * 64) + (Convert.ToInt32(hasMoved) * 1024);
        }

        public override void Moved(Position move, GameState board) {
            if (Math.Abs(move.Row - Row) == 2) {
                if (white) {
                    board.state.Add(new Position((byte)(Row - 1), Column), new EnPassantPlaceholder(new Position((byte)(Row - 1), Column), white, board));
                }
                else {
                    board.state.Add(new Position((byte)(Row + 1), Column), new EnPassantPlaceholder(new Position((byte)(Row + 1), Column), white, board));
                }
            }
            hasMoved = true;
            base.Moved(move, board);
        }


    }

    class EnPassantPlaceholder : Piece {
        public override byte type => Game.PASSANT;
        public override List<Position> Moves(GameState board) {
            return new List<Position>(); //Return an empty list, the placeholder can't move anywhere
        }

        public override bool CanCapture(Piece other) {
            return true;
        }

        public EnPassantPlaceholder(Position Position, bool isWhite, GameState game) {
            position = Position;
            white = isWhite;
            game.TurnCompleted += placeholder_TurnCompleted;
            
        }

        void placeholder_TurnCompleted(object sender, GameState args) {
            Piece piece;
            if (args.state.TryGetValue(position, out piece)) {
                if (piece == this) {
                    args.state.Remove(position);
                }
                else if (white && !piece.white && piece.type == Game.PAWN) {
                    args.state.Remove(new Position((byte)(Row - 1), Column));
                }
                else if (!white && piece.white && piece.type == Game.PAWN) {
                    args.state.Remove(new Position((byte)(Row + 1), Column));
                }
            }
        }
    }
    

    class Knight : Piece
    {
        public override byte type { get => Game.KNIGHT; } //Setting the type property of all knights to KNIGHT
        //I should explain what these keywords do:
        //new tells the compiler that I want to override the implementation of the same variable from the base class
        //public means the variable can be accessed by code from outside this class
        //static means this is the same for all instances of Knight
        //And int, predictably, means integer

        public Knight(Position startPosition, bool isWhite) //I don't think its necessary to explain these over and over again since they're the same for most of the pieces, so I'll just explain the differences
        {
            position = startPosition;
            white = isWhite;
        }

        public override List<Position> Moves(GameState board)
        {
            List<Position> output = new List<Position>();
            //Declare and instantiate new list called output
            for (int column = Column - 2; column <= Column + 2; column += 1) {
                for (int row = Row - 2; row <= Row + 2; row += 1) {
                    if ((row != Row) && (column != Column) && ((Math.Abs(Row - row) + Math.Abs(Column - column)) == 3)) {
                        Piece target;
                        if (board.state.TryGetValue(new Position((byte)row, (byte)column), out target) == false) {
                            output.Add(new Position((byte)row, (byte)column));
                        }
                        else if (target.CanCapture(this)) {
                            output.Add(new Position((byte)row, (byte)column));
                        }
                        
                    }
                }
            }
            


            return output;
        }
    }

    class Bishop : Piece
    {
        public override byte type { get => Game.BISHOP; }

        public Bishop(Position startPosition, bool isWhite)
        {
            position = startPosition;
            white = isWhite;
        }

        public override List<Position> Moves(GameState board) {

            List<Position> output = new List<Position>();
            
            for (byte i = 1; i <= 7; i++) {
                Position move = new Position((byte)(Row + i), (byte)(Column + i));
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                }
                output.Add(move);
            }

            for (byte i = 1; i <= 7; i++) {
                Position move = new Position((byte)(Row - i), (byte)(Column + i));
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                }
                output.Add(move);
            }

            for (byte i = 1; i <= 7; i++) {
                Position move = new Position((byte)(Row + i), (byte)(Column - i));
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                }
                output.Add(move);
            }

            for (byte i = 1; i <= 7; i++) {
                Position move = new Position((byte)(Row - i), (byte)(Column - i));
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                }
                output.Add(move);
            }

            return output;
        }
    }

    class Rook : Piece
    {
        public override byte type { get => Game.ROOK; }
        public bool canCastle; //Since the rook needs to keep track of whether or not it has moved on account of being involved in castling, it get a bool called canCastle for that purpose
        //Confusingly, overriding properties from the base class requires the new keyword or the compiler complains (though it does actually still work), but creating new properties does not
        //require the new keyword

        public Rook(Position startPosition, bool isWhite, bool castle = true) //Much like python, default inputs for a function can be set by setting the variable in the function definition
        {
            position = startPosition;
            white = isWhite;
            canCastle = castle; //Also copying the castle value
        }

        public override List<Position> Moves(GameState board)
        {
            
            List<Position> output = new List<Position>();
            for (byte column = (byte)(Column + 1); column <= 7; column++) {
                Position move = new Position(Row, column);
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                }
                output.Add(move);
            }

            for (byte column = (byte)(Column - 1); column != 255; column--) {
                Position move = new Position(Row, column);
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                }
                output.Add(move);
            }

            for (byte row = (byte)(Row + 1); row <= 7; row++) {
                Position move = new Position(row, Column);
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                }
                output.Add(move);
            }

            for (byte row = (byte)(Row - 1); row != 255; row--) {
                Position move = new Position(row, Column);
                Piece tile;
                if (board.state.TryGetValue(move, out tile)) {
                    if (tile.CanCapture(this)) {
                        output.Add(move);
                    }
                    if (tile.type != Game.PASSANT) {
                        break;
                    }
                    
                }
                output.Add(move);
            }
            return output;
        }

        public override int GetHashCode() {
            return ((position.Row * 8) + position.Column) + (RenderID * 64) + (Convert.ToInt32(canCastle) * 1024);
        }

        public override void Moved(Position move, GameState board) {
            base.Moved(move, board);
            canCastle = false;
        }
    }

    class Queen : Piece
    {
        public override byte type { get => Game.QUEEN; }

        public Queen(Position startPosition, bool isWhite)
        {
            position = startPosition;
            white = isWhite;
        }

        public override List<Position> Moves(GameState board)
        {
            List<Position> output = new List<Position>();
            Bishop bishop = new Bishop(position, white);
            Rook rook = new Rook(position, white);
            output.AddRange(bishop.Moves(board));
            output.AddRange(rook.Moves(board));
            //Simply add together the moves of a bishop and the moves of a rook in the queen's position


            return output;
        }
    }

    class King : Piece
    {
        public override byte type { get => Game.KING; }
        public bool canCastle; //The king also needs to keep track of its eligibility to castle
        //The king pieces do not directly worry about check and checkmate, that's the game state's job

        public King(Position startPosition, bool isWhite, bool castle = true)
        {
            position = startPosition;
            white = isWhite;
            canCastle = castle;
        }

        public override List<Position> Moves(GameState board)
        {
            List<Position> output = new List<Position>();
            
            for (byte column = (byte)(Column - 1); column <= Column + 1; column++) {
                for (byte row = (byte)(Row - 1); row <= Row + 1; row++) {
                    if ((row != Row) || (column != Column)) {
                        if (!board.Threatened(new Position(row, column), white)) {
                            Piece target;
                            if (board.state.TryGetValue(new Position(row, column), out target)) {
                                if (target.white != white) {
                                    output.Add(new Position(row, column));
                                }
                            }
                        }
                    }
                }
            }
            //All the normal king moves
            
            if (canCastle) {
                Piece kingSide;
                Piece queenSide;
                byte row = (byte)(7 * Convert.ToInt32(white)); //Calculate the home row
                if (board.state.TryGetValue(new Position(row, 0), out queenSide)) {
                    if ((queenSide.type == Game.ROOK) && (queenSide.white == white) && queenSide.canCastle) {
                        if (!board.state.ContainsKey(new Position(row, 3)) && !board.state.ContainsKey(new Position(row, 2)) && !board.state.ContainsKey(new Position(row, 1)) &&
                            !board.Threatened(new Position(row, 3), white) && !board.Threatened(new Position(row, 2), white) && !board.Threatened(new Position(row, 4), white)
                            ) { //Check that all the positions between the king and the rook are empty, and all the positions from the king to one before the rook are not threatened.
                            output.Add(new Position(row, 2));
                        }
                    }
                }
            }
            
            




            return output;
        }

        public override int GetHashCode() {
            return ((position.Row * 8) + position.Column) + (RenderID * 64) + (Convert.ToInt32(canCastle) * 1024);
        }

        public override void Moved(Position move, GameState board) {
            base.Moved(move, board);
            canCastle = false;
        }
    }
}
