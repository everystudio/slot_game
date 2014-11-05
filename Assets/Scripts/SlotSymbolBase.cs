using UnityEngine;
using System.Collections;

#pragma warning disable 0414
#pragma warning disable 0108
namespace EveryStudio.SlotGame
{

	[System.Serializable]
	public class TSymbolData{
		public int iId;
		public Sprite sprSprite;
	}


	[System.Serializable]
	[RequireComponent(typeof(SpriteRenderer))]
	public class SlotSymbolBase : MonoBehaviour {

		private Transform m_tfSelf;
		public Transform transform{
			get{
				if(m_tfSelf==null){
					m_tfSelf = gameObject.transform;
				}
				return m_tfSelf;
			}
		}
		private Sprite m_sprImage;
		private SpriteRenderer m_srRender;

		public void Resize( float _fWidth , float _fHeight ){
			float fWidth  = m_srRender.bounds.size.x;
			float fHeight = m_srRender.bounds.size.y;

			transform.localScale = new Vector3(
					transform.localScale.x * _fWidth / fWidth ,
					transform.localScale.y * _fHeight/ fHeight,
					1.0f
				);
			return;
		}

		public void Resize( int _iWidth , int _iHeight ){
			Resize( (float)_iWidth , (float)_iHeight);
			return;
		}

		public void Init( float _fWidth , float _fHeight ){
			m_srRender = GetComponent<SpriteRenderer>();
			Resize(_fWidth,_fHeight);
			return;
		}

		public void SetPos( float _fX , float _fY ){
			transform.localPosition = new Vector3(_fX,_fY,0.0f);
			return;
		}

		public void SetSprite( Sprite _sprImage ){
			if( m_srRender ){
				m_srRender.sprite = _sprImage;
			}
			return;
		}
	}
}
