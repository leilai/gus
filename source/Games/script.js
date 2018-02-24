
var lastDescTarget = null;

function LinkEvents( parameters )
{
	var linkTarget = document.getElementById( parameters.linkId );
	var descTarget = document.getElementById( parameters.descId );
	var descInfo   = document.getElementById( "desc_info" );

	LinkEvents.prototype.onMouseOver = function() {
		descInfo.style.display = "none";
		if ( lastDescTarget != null )
		{
			lastDescTarget.style.display = "none";
		}

		descTarget.style.display = "block";
		lastDescTarget = descTarget;
	}

	if ( linkTarget.addEventListener )
	{
		linkTarget.addEventListener( "mouseover", this.onMouseOver, false );
	}
	else if ( linkTarget.attachEvent )
	{
		linkTarget.attachEvent( "onmouseover", this.onMouseOver );
	}
}

function OnLoadWindow()
{
	document.getElementById( "desc_info" ).innerHTML = "マウスをアイコンに乗せるとゲームの説明が表示されます";

	var oni = new LinkEvents( { linkId: "link_oni", descId: "desc_oni" } );
	var puz = new LinkEvents( { linkId: "link_puz", descId: "desc_puz" } );
	var dun = new LinkEvents( { linkId: "link_dun", descId: "desc_dun" } );
	var oto = new LinkEvents( { linkId: "link_oto", descId: "desc_oto" } );
	var ban = new LinkEvents( { linkId: "link_ban", descId: "desc_ban" } );
	var bit = new LinkEvents( { linkId: "link_bit", descId: "desc_bit" } );
	var eat = new LinkEvents( { linkId: "link_eat", descId: "desc_eat" } );
	var nek = new LinkEvents( { linkId: "link_nek", descId: "desc_nek" } );
	var rpg = new LinkEvents( { linkId: "link_rpg", descId: "desc_rpg" } );
	var naz = new LinkEvents( { linkId: "link_naz", descId: "desc_naz" } );
}

if ( window.addEventListener )
{
	window.addEventListener( "load", OnLoadWindow, false );
}
else if ( window.attachEvent )
{
	window.attachEvent( "onload", OnLoadWindow );
}
