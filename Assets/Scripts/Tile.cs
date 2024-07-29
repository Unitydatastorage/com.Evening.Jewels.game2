using UnityEngine;
using UnityEngine.UI;

namespace MatchThreeEngine
{
    public sealed class Tile : MonoBehaviour
    {
        public int x;
        public int y;
        public Image icon;
        public Button button;
        private TileTypeAsset _type;
        
        public TileTypeAsset Type
        {
            get => _type;

            set
            {
                if (_type == value) return;

                _type = value;

                icon.sprite = _type.sprite;
            }
        }
        
        public TileInfo Data => new TileInfo(x, y, _type.id);
        
        public void Initialize(int x, int y, TileTypeAsset type)
        {
            this.x = x;
            this.y = y;
            Type = type;
            button.onClick.AddListener(OnTileClicked);
        }
        
        private void OnTileClicked()
        {
            Debug.Log($"Tile clicked at ({x}, {y}) with type {_type.id}");
        }
    }
}