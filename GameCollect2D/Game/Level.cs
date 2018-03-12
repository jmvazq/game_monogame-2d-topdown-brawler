using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using GameEngine.Models;
using GameEngine.Sprites;

namespace GameEngine
{
    class Level
    {
        int _tileLength;

        int _rows = 15;
        int _columns = 20;

        readonly Tile[,] _tileMap;
        GameObject[,] _objMap;
        public GameObject[,] ObjMap {
            get
            {
                return _objMap;
            }
        }

        public int Rows
        {
            get
            {
                return _rows;
            }
        }
        public int Columns
        {
            get
            {
                return _columns;
            }
        }

        public Level(Texture2D baseTexture, int columns, int rows, int tileLength)
        {
            this._rows = rows;
            this._columns = columns;
            this._tileLength = tileLength;
            this._tileMap = new Tile[this._columns, this._rows];
            this._objMap = new GameObject[this._columns, this._rows];

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector2 tilePosition = new Vector2(x * this._tileLength, y * tileLength);
                    _tileMap[x, y] = new Tile(baseTexture, tilePosition, Passability.passable);
                }
            }
        }

        public void SetTile(int column, int row, Tile tile)
        {
            tile.Position = new Vector2(column * this._tileLength, row * _tileLength);
            this._tileMap[column, row] = tile;
        }

        public void FillTileRange(int startColumn, int startRow, int endColumn, int endRow, Texture2D texture)
        {
            for (int x = startColumn; x < endColumn; x++)
            {
                for (int y = startRow; y < endRow; y++)
                {
                    SetTile(x, y, new Tile(texture, Vector2.Zero, Passability.passable));
                }
            }
        }

        public void SetObject(int column, int row, GameObject sprite, Passability passability = Passability.block)
        {
            sprite.Position = new Vector2(column * this._tileLength, row * _tileLength);
            sprite.Column = column;
            sprite.Row = row;
            sprite.IsDisplaced = false;
            sprite.Passability = passability;
            this.ObjMap[column, row] = sprite;
        }

        public void FillObjectRange(int startColumn, int startRow, int endColumn, int endRow, Texture2D texture, Passability passability = Passability.block)
        {
            for (int x = startColumn; x < endColumn; x++)
            {
                for (int y = startRow; y < endRow; y++)
                {
                    SetObject(x, y, new GameObject(texture), passability);
                }
            }
        }

        public void RemoveObject(int column, int row)
        {
            GameObject obj = this.ObjMap[column, row];
            obj.IsDisplaced = true;
            this.ObjMap[column, row] = null;
        }

        public void Update(Game game, Camera2D camera, Viewport viewport, GameTime gameTime, Level level)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (Sprite obj in ObjMap)
            {
                if (obj != null)
                    sprites.Add(obj);
            }

            foreach (Tile tile in this._tileMap)
            {
                tile.Update(gameTime, sprites);
            }
            foreach (Sprite obj in this.ObjMap)
            {
                if (obj != null)
                {
                    obj.Update(game, camera, viewport, gameTime, level, sprites);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var tile in this._tileMap)
            {
                tile.Draw(spriteBatch);
            }
            foreach (var obj in this.ObjMap)
            {
                if (obj != null)
                    obj.Draw(spriteBatch);
            }
        }
    }
}
