Unity Runtime SpriteSheets Generator
====================================

Unity and plugins provide many great ways to build Sprite Sheets. However they're used directly into Unity Editor or with an external software which is perfect in many case, but none provide the ability to generate SpriteSheets at runtime.

The RectanglePacking algorithm is a port of the AS3 version made by [Ville Koskela](https://github.com/villekoskelaorg/RectanglePacking). Assets used in the demo come from [Kenney](http://kenney.nl/).

You could combine the generated Sprite Sheets.png with a [pngquant](https://pngquant.org/) compression via this script [PngQuantNativeProcess](https://github.com/DaVikingCode/PngQuantNativeProcess).

Example:
--------
Add the `AssetPacker` component to your GameObject:  
![AssetPacker](http://davikingcode.com/blog/wp-content/uploads/2017/01/AssetPacker.png)
```csharp
using DaVikingCode.AssetPacker;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AssetPackerExample : MonoBehaviour {
	
	public Image anim;

	AssetPacker assetPacker;
	
	void Start () {

		string[] files = Directory.GetFiles(Application.persistentDataPath + "/Textures", "*.png");

		assetPacker = GetComponent<AssetPacker>();

		assetPacker.OnProcessCompleted.AddListener(LaunchAnimations);

		assetPacker.AddTexturesToPack(files);
		assetPacker.Process();
	}

	void LaunchAnimations() {

		StartCoroutine(LoadAnimation());
	}

	IEnumerator LoadAnimation() {

		Sprite[] sprites = assetPacker.GetSprites("walking");

		int j = 0;
		while (j < sprites.Length) {

			anim.sprite = sprites[j++];

			yield return new WaitForSeconds(0.1f);

			if (j == sprites.Length)
				j = 0;
		}
	}
}

```

扩展:
--------

> - 原工程中有个致命的缺陷，在移动端频繁的调用**Texture.SetPixels32**，**Texture.Apply**以及**Sprite.Create**时耗时非常严重，尤其尺寸很大的图片，这些问题无法避免的。我们项目现在使用到物品Icon的尺寸是138*138，而背包中同时出现的不同物品能达到40个。当执行完合并后耗时需要好几秒。这根本就不能接受的。

> - 我尝试过使用Graphics.CopyTexture方式，但他的兼容性很差。我还没找到规律，处理同尺寸的Icon合并勉强可以，当大量的不同尺寸合并到一起时就有问题了。

> - RectanglePackingForGL Demo使用的GL方式，原理就是把一个图元先缓存到一个Material中进行采样，在通过GL方式把顶点和纹理拷贝到RenderTexture中。并且把Image组件改成RawImage组件，性能提高了很多，具体没有进行测试。当然也有缺陷，RawImage不能进行九宫格缩放。可以根据自己项目需求而定。个人认为既然使用了动态合并图集方式，九宫格缩放会用不到。