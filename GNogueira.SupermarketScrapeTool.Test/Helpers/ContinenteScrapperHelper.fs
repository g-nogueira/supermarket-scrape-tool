namespace GNogueira.SupermarketScrapeTool.Tests

open GNogueira.SupermarketScrapeTool.Models

module ContinenteScrapperHelper =

    let invalidHtml = "<htmll><</html>"

    let productListLength = 1
    let priceUnit = PriceUnit.Kg
    let productName = "Banana"
    let productPrice = 1.25
    let productId = "2597619"
    let productUrl = "https://www.continente.pt/produto/banana-continente-2597619.html"
    let productBrand = "Continente"

    let productImageUrl =
        "https://www.continente.pt/dw/image/v2/BDVS_PRD/on/demandware.static/-/Sites-col-master-catalog/default/dwcd554331/images/col/259/2597619-frente.jpg?sw=280&amp;sh=280"

    let productListResponse =
        """
<span class="page-placeholder-0" data-start="0" data-size="1.0" data-srule="Continente"
	data-filters="[{&quot;key&quot;:&quot;pmin&quot;,&quot;value&quot;:&quot;0.01&quot;}]">
</span>
<div class="col-12 col-sm-3 col-lg-2 productTile" data-idx="1">
	<div class="product" data-pid="2597619">
		<div class="product-tile pid-2597619   ct-product-tile col-product-tile " data-delay-time="3000"
			data-variants-mapping="[]" data-brandID="col" data-in-cart-msg="no carrinho"
			data-one-product-added="Adicionado ao carrinho" data-remove-from-cart-msg="Removido do carrinho"
			data-stay-open="6000.0"
			data-product-tile-impression='{&quot;name&quot;:&quot;Banana&quot;,&quot;id&quot;:&quot;2597619&quot;,&quot;price&quot;:1.25,&quot;brand&quot;:&quot;Continente&quot;,&quot;category&quot;:&quot;Frutas e Legumes/Frutas/Banana, Ma&ccedil;&atilde; e Pera&quot;,&quot;variant&quot;:&quot;&quot;,&quot;channel&quot;:&quot;col&quot;}'>
			<div
				class="ct-inner-tile-wrap col-inner-tile-wrap row no-gutters justify-content-center align-content-start">
				<!-- dwMarker="product" dwContentID="a10793d1bfcb84a433ea5c0a2f" -->
				<div class="ct-image-container col-4 col-sm"
					data-confirmation-image="{&quot;url&quot;:&quot;https://www.continente.pt/dw/image/v2/BDVS_PRD/on/demandware.static/-/Sites-col-master-catalog/default/dwcd554331/images/col/259/2597619-frente.jpg?sw=87&amp;sh=80&quot;,&quot;fallbackimage&quot;:{&quot;url&quot;:&quot;https://www.continente.pt/dw/image/v2/BDVS_PRD/on/demandware.static/Sites-continente-Site/-/default/dwc9c6bbd2/images/noimagelarge_product.png?sw=87&amp;sh=80&quot;,&quot;title&quot;:&quot;Banana&quot;,&quot;alt&quot;:&quot;Banana&quot;},&quot;title&quot;:&quot;Banana&quot;,&quot;alt&quot;:&quot;Banana&quot;}">
					<a href="https://www.continente.pt/produto/banana-continente-2597619.html">
						<picture class="intrinsic intrinsic--square intrinsic--square">
							<img class="ct-tile-image lazyload hidden" id="" loading="lazy"
								onerror="this.onerror=null;this.src='/on/demandware.static/Sites-continente-Site/-/default/dwc9c6bbd2/images/noimagelarge_product.png';"
								data-src="https://www.continente.pt/dw/image/v2/BDVS_PRD/on/demandware.static/-/Sites-col-master-catalog/default/dwcd554331/images/col/259/2597619-frente.jpg?sw=280&amp;sh=280"
								alt="Banana" title="Banana" />
						</picture>
					</a>
					<div class="color-swatches d-flex" style="margin-left: 5px;">
						<div class="swatches d-flex">
						</div>
					</div>
				</div>
				<div class="ct-tile-body col col-sm-auto ">
					<div class="ct-pdp-details ">
						<div class="ct-pdp-link col-pdp-link">
							<a class="pwc-tile--description col-tile--description"
								href="https://www.continente.pt/produto/banana-continente-2597619.html">Banana</a>
							<div class="pwc-tile--favorite-top col-tile--favorite-top d-sm-none">
								<button
									class="wishlist-toggler col-wishlist-toggler guest-mode inactive button button--tertiary col-button--tertiary"
									data-pid="2597619"
									data-add-url="https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Wishlist-AddProduct?pid=2597619"
									data-toggle='tooltip' aria-label='Adicionar aos favoritos '
									data-original-title='Adicionar aos favoritos' data-placement='top'>
									<svg class="active-content" width="100%" height="100%" viewBox="0 0 20 22"
										fill="none" xmlns="http://www.w3.org/2000/svg">
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M0 1.53052C0 0.685236 0.692171 0 1.54601 0H15.0736C15.9274 0 16.6196 0.685235 16.6196 1.53052V13.4064L15.8127 13.922L14.2658 14.2658L12.7189 13.4064L10.6054 14.3818C10.4994 14.2451 10.3327 14.157 10.1452 14.157H6.76329C6.4431 14.157 6.18354 14.414 6.18354 14.731C6.18354 15.0479 6.4431 15.3049 6.76329 15.3049H10.1452C10.2478 15.3049 10.3441 15.2785 10.4277 15.2322L10.3126 16.8439L13.2345 20.2793H1.54601C0.69217 20.2793 0 19.5941 0 18.7488V1.53052ZM6.18354 5.5483C6.18354 5.23132 6.4431 4.97436 6.76329 4.97436H12.3676C12.6878 4.97436 12.9473 5.23132 12.9473 5.5483C12.9473 5.86528 12.6878 6.12224 12.3676 6.12224H6.76329C6.4431 6.12224 6.18354 5.86528 6.18354 5.5483ZM6.76329 11.0961C6.4431 11.0961 6.18354 11.3531 6.18354 11.6701C6.18354 11.987 6.4431 12.244 6.76329 12.244H12.3676C12.6878 12.244 12.9473 11.987 12.9473 11.6701C12.9473 11.3531 12.6878 11.0961 12.3676 11.0961H6.76329ZM6.18354 8.60985C6.18354 8.29287 6.4431 8.03591 6.76329 8.03591H12.3676C12.6878 8.03591 12.9473 8.29287 12.9473 8.60985C12.9473 8.92684 12.6878 9.1838 12.3676 9.1838H6.76329C6.4431 9.1838 6.18354 8.92684 6.18354 8.60985ZM3.67118 5.5483C3.67118 5.23132 3.93075 4.97436 4.25093 4.97436H4.63743C4.95762 4.97436 5.21719 5.23132 5.21719 5.5483C5.21719 5.86528 4.95762 6.12224 4.63743 6.12224H4.25093C3.93075 6.12224 3.67118 5.86528 3.67118 5.5483ZM4.25093 11.0961C3.93075 11.0961 3.67118 11.3531 3.67118 11.6701C3.67118 11.987 3.93075 12.244 4.25093 12.244H4.63743C4.95762 12.244 5.21719 11.987 5.21719 11.6701C5.21719 11.3531 4.95762 11.0961 4.63743 11.0961H4.25093ZM3.67118 8.60985C3.67118 8.29287 3.93075 8.03591 4.25093 8.03591H4.63743C4.95762 8.03591 5.21719 8.29287 5.21719 8.60985C5.21719 8.92684 4.95762 9.1838 4.63743 9.1838H4.25093C3.93075 9.1838 3.67118 8.92684 3.67118 8.60985ZM4.25093 14.157C3.93075 14.157 3.67118 14.414 3.67118 14.731C3.67118 15.0479 3.93075 15.3049 4.25093 15.3049H4.63743C4.95762 15.3049 5.21719 15.0479 5.21719 14.731C5.21719 14.414 4.95762 14.157 4.63743 14.157H4.25093Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M14.6252 13.6363C13.4918 12.6215 11.7817 12.5264 10.6569 13.4792C9.12421 14.7764 9.34841 16.8912 10.4409 18.0122L14.0159 21.6742C14.2197 21.8832 14.4928 22 14.7822 22C15.0737 22 15.3448 21.8852 15.5486 21.6762L19.1236 18.0142C20.214 16.8933 20.4423 14.7785 18.9075 13.4792C17.7827 12.5283 16.0724 12.6217 14.9391 13.6365C14.8855 13.6845 14.8331 13.7345 14.7822 13.7866C14.7313 13.7345 14.6789 13.6843 14.6252 13.6363ZM18.1546 14.3522C19.0973 15.1508 18.9962 16.4896 18.2891 17.2173C18.2888 17.2175 18.2886 17.2177 18.2884 17.218L14.7832 20.8084L11.2753 17.2151C11.2752 17.215 11.2751 17.2149 11.2749 17.2148C10.5677 16.4887 10.4678 15.1495 11.4104 14.3518L11.4108 14.3514C12.0745 13.7892 13.2018 13.82 13.9488 14.5846L14.7822 15.4378L15.6157 14.5846C16.3617 13.8211 17.4896 13.79 18.1545 14.3521"
											fill="#EB0203" />
									</svg>
									<svg class="inactive-content" width="100%" height="100%" viewBox="0 0 20 23"
										fill="none" xmlns="http://www.w3.org/2000/svg">
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 5.77914C6.18408 5.46223 6.44363 5.20532 6.7638 5.20532H12.3678C12.6879 5.20532 12.9475 5.46223 12.9475 5.77914C12.9475 6.09606 12.6879 6.35297 12.3678 6.35297H6.7638C6.44363 6.35297 6.18408 6.09606 6.18408 5.77914Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 11.9034C6.18408 11.5865 6.44363 11.3296 6.7638 11.3296H12.3678C12.6879 11.3296 12.9475 11.5865 12.9475 11.9034C12.9475 12.2202 12.6879 12.4771 12.3678 12.4771H6.7638C6.44363 12.4771 6.18408 12.2202 6.18408 11.9034Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 8.84116C6.18408 8.52424 6.44363 8.26733 6.7638 8.26733H12.3678C12.6879 8.26733 12.9475 8.52424 12.9475 8.84116C12.9475 9.15807 12.6879 9.41498 12.3678 9.41498H6.7638C6.44363 9.41498 6.18408 9.15807 6.18408 8.84116Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 14.9655C6.18408 14.6486 6.44363 14.3917 6.7638 14.3917H10.1455C10.4657 14.3917 10.7252 14.6486 10.7252 14.9655C10.7252 15.2825 10.4657 15.5394 10.1455 15.5394H6.7638C6.44363 15.5394 6.18408 15.2825 6.18408 14.9655Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 5.77914C3.67139 5.46223 3.93094 5.20532 4.25111 5.20532H4.63759C4.95776 5.20532 5.21731 5.46223 5.21731 5.77914C5.21731 6.09606 4.95776 6.35297 4.63759 6.35297H4.25111C3.93094 6.35297 3.67139 6.09606 3.67139 5.77914Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 11.9034C3.67139 11.5865 3.93094 11.3296 4.25111 11.3296H4.63759C4.95776 11.3296 5.21731 11.5865 5.21731 11.9034C5.21731 12.2202 4.95776 12.4771 4.63759 12.4771H4.25111C3.93094 12.4771 3.67139 12.2202 3.67139 11.9034Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 8.84116C3.67139 8.52424 3.93094 8.26733 4.25111 8.26733H4.63759C4.95776 8.26733 5.21731 8.52424 5.21731 8.84116C5.21731 9.15807 4.95776 9.41498 4.63759 9.41498H4.25111C3.93094 9.41498 3.67139 9.15807 3.67139 8.84116Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 14.9655C3.67139 14.6486 3.93094 14.3917 4.25111 14.3917H4.63759C4.95776 14.3917 5.21731 14.6486 5.21731 14.9655C5.21731 15.2825 4.95776 15.5394 4.63759 15.5394H4.25111C3.93094 15.5394 3.67139 15.2825 3.67139 14.9655Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M0 1.76033C0 0.914738 0.692131 0.229248 1.54592 0.229248H15.0727C15.9265 0.229248 16.6186 0.914738 16.6186 1.76033V11.2818C16.6186 11.5989 16.3591 11.8559 16.0389 11.8559C15.7187 11.8559 15.4592 11.5989 15.4592 11.2818V1.76033C15.4592 1.54893 15.2862 1.37756 15.0727 1.37756H1.54592C1.33247 1.37756 1.15944 1.54893 1.15944 1.76033V18.985C1.15944 19.1964 1.33247 19.3678 1.54592 19.3678H9.27551C9.59568 19.3678 9.85523 19.6249 9.85523 19.942C9.85523 20.2591 9.59568 20.5161 9.27551 20.5161H1.54592C0.692131 20.5161 0 19.8306 0 18.985V1.76033Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M14.6254 13.8718C13.4921 12.8567 11.7821 12.7615 10.6574 13.7147C9.12472 15.0124 9.34891 17.128 10.4413 18.2493L14.0161 21.9126C14.2199 22.1217 14.493 22.2386 14.7824 22.2386C15.0739 22.2386 15.3449 22.1238 15.5487 21.9147L19.1235 18.2514C20.2139 17.13 20.4422 15.0144 18.9075 13.7147C17.7827 12.7634 16.0726 12.8569 14.9392 13.872C14.8856 13.92 14.8333 13.9701 14.7824 14.0222C14.7314 13.97 14.6791 13.9199 14.6254 13.8718ZM18.1546 14.588C19.0972 15.3869 18.9961 16.7262 18.2891 17.4541C18.2888 17.4543 18.2886 17.4546 18.2884 17.4548L14.7834 21.0466L11.2757 17.452C11.2756 17.4519 11.2755 17.4517 11.2753 17.4516C10.5681 16.7252 10.4682 15.3856 11.4108 14.5876L11.4112 14.5872C12.0748 14.0248 13.202 14.0556 13.949 14.8205L14.7824 15.674L15.6158 14.8205C16.3618 14.0567 17.4896 14.0256 18.1545 14.5879"
											fill="#EB0203" />
									</svg>
								</button>
							</div>
						</div>
						<div class="col-tile--brand-wrapper">
							<p class="pwc-tile--brand col-tile--brand">Continente</p>
						</div>
						<p class="pwc-tile--quantity col-tile--quantity">Quant. M&iacute;nima = 600 gr (3 un)</p>
					</div>
					<div
						class="ct-tile-bottom row no-gutters justify-content-between ct-tile-bottom-padding-bottom-grid">
						<div class="pwc-price-wrap col-8 col-sm-12 ">
							<div class="price js-product-price">
								<div class="prices-wrapper">
									<span class="sales pwc-tile--price-primary col-tile--price-primary ">
										<span class="value" content="1.25">
											<span class="ct-price-formatted">
												&euro;1,25
											</span>
											<span class="pwc-m-unit">
												/kg
											</span>
										</span>
									</span>
								</div>
								<div class="pwc-tile--price-secondary col-tile--price-secondary">
									<span class="ct-price-value">
										&#8364;0,25
									</span>
									<span class="pwc-m-unit">
										/un
									</span>
								</div>
							</div>
						</div>
						<div class="dual-badge">
						</div>
						<div
							class="pwc-tile-buy-section d-flex justify-content-center no-gutters pwc-tile-buy-section-padding col-auto col-sm-12 pwc-cta-align-bottom">
							<div class="col-2 d-sm-flex d-none">
								<button
									class="wishlist-toggler col-wishlist-toggler guest-mode inactive button button--tertiary col-button--tertiary"
									data-pid="2597619"
									data-add-url="https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Wishlist-AddProduct?pid=2597619"
									data-toggle='tooltip' aria-label='Adicionar aos favoritos '
									data-original-title='Adicionar aos favoritos' data-placement='top'>
									<svg class="active-content" width="100%" height="100%" viewBox="0 0 20 22"
										fill="none" xmlns="http://www.w3.org/2000/svg">
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M0 1.53052C0 0.685236 0.692171 0 1.54601 0H15.0736C15.9274 0 16.6196 0.685235 16.6196 1.53052V13.4064L15.8127 13.922L14.2658 14.2658L12.7189 13.4064L10.6054 14.3818C10.4994 14.2451 10.3327 14.157 10.1452 14.157H6.76329C6.4431 14.157 6.18354 14.414 6.18354 14.731C6.18354 15.0479 6.4431 15.3049 6.76329 15.3049H10.1452C10.2478 15.3049 10.3441 15.2785 10.4277 15.2322L10.3126 16.8439L13.2345 20.2793H1.54601C0.69217 20.2793 0 19.5941 0 18.7488V1.53052ZM6.18354 5.5483C6.18354 5.23132 6.4431 4.97436 6.76329 4.97436H12.3676C12.6878 4.97436 12.9473 5.23132 12.9473 5.5483C12.9473 5.86528 12.6878 6.12224 12.3676 6.12224H6.76329C6.4431 6.12224 6.18354 5.86528 6.18354 5.5483ZM6.76329 11.0961C6.4431 11.0961 6.18354 11.3531 6.18354 11.6701C6.18354 11.987 6.4431 12.244 6.76329 12.244H12.3676C12.6878 12.244 12.9473 11.987 12.9473 11.6701C12.9473 11.3531 12.6878 11.0961 12.3676 11.0961H6.76329ZM6.18354 8.60985C6.18354 8.29287 6.4431 8.03591 6.76329 8.03591H12.3676C12.6878 8.03591 12.9473 8.29287 12.9473 8.60985C12.9473 8.92684 12.6878 9.1838 12.3676 9.1838H6.76329C6.4431 9.1838 6.18354 8.92684 6.18354 8.60985ZM3.67118 5.5483C3.67118 5.23132 3.93075 4.97436 4.25093 4.97436H4.63743C4.95762 4.97436 5.21719 5.23132 5.21719 5.5483C5.21719 5.86528 4.95762 6.12224 4.63743 6.12224H4.25093C3.93075 6.12224 3.67118 5.86528 3.67118 5.5483ZM4.25093 11.0961C3.93075 11.0961 3.67118 11.3531 3.67118 11.6701C3.67118 11.987 3.93075 12.244 4.25093 12.244H4.63743C4.95762 12.244 5.21719 11.987 5.21719 11.6701C5.21719 11.3531 4.95762 11.0961 4.63743 11.0961H4.25093ZM3.67118 8.60985C3.67118 8.29287 3.93075 8.03591 4.25093 8.03591H4.63743C4.95762 8.03591 5.21719 8.29287 5.21719 8.60985C5.21719 8.92684 4.95762 9.1838 4.63743 9.1838H4.25093C3.93075 9.1838 3.67118 8.92684 3.67118 8.60985ZM4.25093 14.157C3.93075 14.157 3.67118 14.414 3.67118 14.731C3.67118 15.0479 3.93075 15.3049 4.25093 15.3049H4.63743C4.95762 15.3049 5.21719 15.0479 5.21719 14.731C5.21719 14.414 4.95762 14.157 4.63743 14.157H4.25093Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M14.6252 13.6363C13.4918 12.6215 11.7817 12.5264 10.6569 13.4792C9.12421 14.7764 9.34841 16.8912 10.4409 18.0122L14.0159 21.6742C14.2197 21.8832 14.4928 22 14.7822 22C15.0737 22 15.3448 21.8852 15.5486 21.6762L19.1236 18.0142C20.214 16.8933 20.4423 14.7785 18.9075 13.4792C17.7827 12.5283 16.0724 12.6217 14.9391 13.6365C14.8855 13.6845 14.8331 13.7345 14.7822 13.7866C14.7313 13.7345 14.6789 13.6843 14.6252 13.6363ZM18.1546 14.3522C19.0973 15.1508 18.9962 16.4896 18.2891 17.2173C18.2888 17.2175 18.2886 17.2177 18.2884 17.218L14.7832 20.8084L11.2753 17.2151C11.2752 17.215 11.2751 17.2149 11.2749 17.2148C10.5677 16.4887 10.4678 15.1495 11.4104 14.3518L11.4108 14.3514C12.0745 13.7892 13.2018 13.82 13.9488 14.5846L14.7822 15.4378L15.6157 14.5846C16.3617 13.8211 17.4896 13.79 18.1545 14.3521"
											fill="#EB0203" />
									</svg>
									<svg class="inactive-content" width="100%" height="100%" viewBox="0 0 20 23"
										fill="none" xmlns="http://www.w3.org/2000/svg">
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 5.77914C6.18408 5.46223 6.44363 5.20532 6.7638 5.20532H12.3678C12.6879 5.20532 12.9475 5.46223 12.9475 5.77914C12.9475 6.09606 12.6879 6.35297 12.3678 6.35297H6.7638C6.44363 6.35297 6.18408 6.09606 6.18408 5.77914Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 11.9034C6.18408 11.5865 6.44363 11.3296 6.7638 11.3296H12.3678C12.6879 11.3296 12.9475 11.5865 12.9475 11.9034C12.9475 12.2202 12.6879 12.4771 12.3678 12.4771H6.7638C6.44363 12.4771 6.18408 12.2202 6.18408 11.9034Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 8.84116C6.18408 8.52424 6.44363 8.26733 6.7638 8.26733H12.3678C12.6879 8.26733 12.9475 8.52424 12.9475 8.84116C12.9475 9.15807 12.6879 9.41498 12.3678 9.41498H6.7638C6.44363 9.41498 6.18408 9.15807 6.18408 8.84116Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M6.18408 14.9655C6.18408 14.6486 6.44363 14.3917 6.7638 14.3917H10.1455C10.4657 14.3917 10.7252 14.6486 10.7252 14.9655C10.7252 15.2825 10.4657 15.5394 10.1455 15.5394H6.7638C6.44363 15.5394 6.18408 15.2825 6.18408 14.9655Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 5.77914C3.67139 5.46223 3.93094 5.20532 4.25111 5.20532H4.63759C4.95776 5.20532 5.21731 5.46223 5.21731 5.77914C5.21731 6.09606 4.95776 6.35297 4.63759 6.35297H4.25111C3.93094 6.35297 3.67139 6.09606 3.67139 5.77914Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 11.9034C3.67139 11.5865 3.93094 11.3296 4.25111 11.3296H4.63759C4.95776 11.3296 5.21731 11.5865 5.21731 11.9034C5.21731 12.2202 4.95776 12.4771 4.63759 12.4771H4.25111C3.93094 12.4771 3.67139 12.2202 3.67139 11.9034Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 8.84116C3.67139 8.52424 3.93094 8.26733 4.25111 8.26733H4.63759C4.95776 8.26733 5.21731 8.52424 5.21731 8.84116C5.21731 9.15807 4.95776 9.41498 4.63759 9.41498H4.25111C3.93094 9.41498 3.67139 9.15807 3.67139 8.84116Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M3.67139 14.9655C3.67139 14.6486 3.93094 14.3917 4.25111 14.3917H4.63759C4.95776 14.3917 5.21731 14.6486 5.21731 14.9655C5.21731 15.2825 4.95776 15.5394 4.63759 15.5394H4.25111C3.93094 15.5394 3.67139 15.2825 3.67139 14.9655Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M0 1.76033C0 0.914738 0.692131 0.229248 1.54592 0.229248H15.0727C15.9265 0.229248 16.6186 0.914738 16.6186 1.76033V11.2818C16.6186 11.5989 16.3591 11.8559 16.0389 11.8559C15.7187 11.8559 15.4592 11.5989 15.4592 11.2818V1.76033C15.4592 1.54893 15.2862 1.37756 15.0727 1.37756H1.54592C1.33247 1.37756 1.15944 1.54893 1.15944 1.76033V18.985C1.15944 19.1964 1.33247 19.3678 1.54592 19.3678H9.27551C9.59568 19.3678 9.85523 19.6249 9.85523 19.942C9.85523 20.2591 9.59568 20.5161 9.27551 20.5161H1.54592C0.692131 20.5161 0 19.8306 0 18.985V1.76033Z"
											fill="#EB0203" />
										<path fill-rule="evenodd" clip-rule="evenodd"
											d="M14.6254 13.8718C13.4921 12.8567 11.7821 12.7615 10.6574 13.7147C9.12472 15.0124 9.34891 17.128 10.4413 18.2493L14.0161 21.9126C14.2199 22.1217 14.493 22.2386 14.7824 22.2386C15.0739 22.2386 15.3449 22.1238 15.5487 21.9147L19.1235 18.2514C20.2139 17.13 20.4422 15.0144 18.9075 13.7147C17.7827 12.7634 16.0726 12.8569 14.9392 13.872C14.8856 13.92 14.8333 13.9701 14.7824 14.0222C14.7314 13.97 14.6791 13.9199 14.6254 13.8718ZM18.1546 14.588C19.0972 15.3869 18.9961 16.7262 18.2891 17.4541C18.2888 17.4543 18.2886 17.4546 18.2884 17.4548L14.7834 21.0466L11.2757 17.452C11.2756 17.4519 11.2755 17.4517 11.2753 17.4516C10.5681 16.7252 10.4682 15.3856 11.4108 14.5876L11.4112 14.5872C12.0748 14.0248 13.202 14.0556 13.949 14.8205L14.7824 15.674L15.6158 14.8205C16.3618 14.0567 17.4896 14.0256 18.1545 14.5879"
											fill="#EB0203" />
									</svg>
								</button>
							</div>
							<div class="col-sm-10 col-12">
								<div class="ct-tile-quantity d-none" data-pid="2597619">
									<input type="hidden" class="add-to-cart-url"
										value="https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Cart-AddProduct" />
									<div class="d-none tile-delete-confirmation-btn" data-pid="2597619"
										data-delay-time="3000" data-in-cart-msg="no carrinho"
										data-one-product-added="Adicionado ao carrinho"
										data-remove-from-cart-msg="Removido do carrinho" data-stay-open="6000.0"
										data-confirmation-image="{&quot;url&quot;:&quot;https://www.continente.pt/dw/image/v2/BDVS_PRD/on/demandware.static/-/Sites-col-master-catalog/default/dwcd554331/images/col/259/2597619-frente.jpg?sw=87&amp;sh=80&quot;,&quot;fallbackimage&quot;:{&quot;url&quot;:&quot;https://www.continente.pt/dw/image/v2/BDVS_PRD/on/demandware.static/Sites-continente-Site/-/default/dwc9c6bbd2/images/noimagelarge_product.png?sw=87&amp;sh=80&quot;,&quot;title&quot;:&quot;Banana&quot;,&quot;alt&quot;:&quot;Banana&quot;},&quot;title&quot;:&quot;Banana&quot;,&quot;alt&quot;:&quot;Banana&quot;}">
									</div>
									<div class="ct-tile-quantity-update quantity-update product-qty-vue"
										data-remove-action="https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Cart-RemoveProductLineItem"
										data-action="https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Cart-UpdateQuantity"
										data-dimension-update="https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Cart-DimensionUpdate"
										data-pid="2597619" data-uuid="a10793d1bfcb84a433ea5c0a2f" data-add-delay="500.0"
										data-measure-options="{&quot;hasConversionRate&quot;:true,&quot;hasAlternativeSaleUnit&quot;:true,&quot;primaryToSecondary&quot;:0.2,&quot;secondaryToPrimary&quot;:1,&quot;unitConversionRate&quot;:0.2,&quot;minOrderQuantity&quot;:0.6,&quot;stepQuantity&quot;:0.2,&quot;primaryunit&quot;:&quot;kg&quot;,&quot;secondaryunit&quot;:&quot;un&quot;,&quot;selectedunit&quot;:&quot;primary&quot;,&quot;maxNumberOfUnitsPerSale&quot;:99}"
										data-price-available="null"
										:data-has-error="isMaxQtyReached || isMinQtyReached || qtySelectedMoreThanOne">
										<template v-show="prodUnits">
											<div class="quantity-input-wrapper col-quantity-input-wrapper has-units">
												<label class="visually-hidden" :for="'quantity-' + pid + additionalTag">
													Quantidade no carrinho
												</label>
												<input :ref="'quantityInput-' + pid + additionalTag"
													class="add-to-cart-quantity form-control pwc-form-input col-form-input text-center col-form-input-quantity"
													inputmode="numeric" :class="{'is-invalid': isInvalid}"
													:data-pid="pid + additionalTag"
													:name="'quantity-' + pid + additionalTag" type="text"
													aria-label="Insira a quantidade" title v-model="displayValue"
													@change="updateQuantity" @focus="updateQtyFormat"
													@blur="restoreQtyFormat" :disabled="!isPriceAvailable" />
												<span class="quantity-icon quantity-icon--minus">
													<button class="decrease-quantity-btn" @click="updateQuantity"
														:disabled="!isPriceAvailable" role="button"
														aria-label="Reduzir quantidade">
														<i class="css-math-minus col-css-math-minus"></i>
													</button>
												</span>
												<span class="quantity-icon quantity-icon--plus">
													<button class="increase-quantity-btn" @click="updateQuantity"
														:disabled="isMaxQtyReached || !isPriceAvailable" role="button"
														aria-label="Aumentar quantidade">
														<i class="css-math-plus col-css-math-plus"></i>
													</button>
													<span class="quantity-btn-disabled-placeholder"></span>
												</span>
												<template v-if="isPDP">
													<span
														class="quantity-error pwc-span-error text-sm-center product-max-allocation"
														v-if="isMaxQtyReached">
														Quantidade m&aacute;xima atingida
													</span>
													<span
														class="quantity-error pwc-span-product col-span-product text-sm-center product-allocation"
														v-if="!isMinQtyReached && !isMaxQtyReached">
														{{quantitySelected}} no carrinho
													</span>
													<span
														class="quantity-error pwc-span-error text-sm-center product-min-allocation"
														:data-pid="pid" v-if="isMinQtyReached && !isMaxQtyReached">
														Quantidade m&iacute;nima atingida
													</span>
												</template>
											</div>
											<template v-if="showNotes">
												<div class="add-note-link-container ">
													<div class="add-note col-add-note">
														<a href="/produto/banana-continente-2597619.html?showNote=true"
															class="ct-add-note-redirect-pdp"
															aria-label="Adicionar coment&aacute;rio">
															<span class="svg-wrapper svg-icon-notes"
																style="width:19px; height:20px">
																<svg class='svg-inline icon-notes' viewBox='0 0 28 30'
																	xmlns='http://www.w3.org/2000/svg' width='100%'
																	height='100%'>
																	<path
																		d='M21.738 0C22.985 0 24 1.04 24 2.319V12.47c0 .469-.37.848-.828.848-.457 0-.827-.38-.827-.848V2.32c0-.343-.273-.622-.607-.622H2.262c-.334 0-.607.279-.607.622V27.68c0 .343.273.622.607.622h7.81c.457 0 .827.38.827.849 0 .468-.37.848-.827.848h-7.81C1.015 30 0 28.96 0 27.681V2.32C0 1.04 1.015 0 2.262 0h19.476zm2.844 16.004l.227.007c.823.053 1.589.39 2.176.964.64.627.993 1.46.993 2.349 0 .888-.353 1.722-.993 2.348l-6.83 6.665c-.11.108-.247.186-.397.226l-5.032 1.363c-.078.021-.16.032-.241.032-.242 0-.478-.097-.647-.266-.23-.231-.314-.566-.218-.874l1.49-4.798c.044-.139.122-.266.227-.369l6.844-6.677c.64-.625 1.493-.97 2.401-.97zm-2.886 3.94l-4.922 4.802-.947 3.048 3.22-.872 4.897-4.78-2.248-2.197zm2.887-2.174c-.425 0-.824.162-1.124.455l-.484.472 2.248 2.197.484-.472c.62-.606.62-1.591 0-2.197l-.117-.104c-.283-.227-.636-.35-1.007-.35zm-10.839.022c.498 0 .904.396.904.883 0 .488-.406.884-.904.884H5.946c-.499 0-.904-.396-.904-.884 0-.487.405-.883.904-.883zm5.042-4.876c.498 0 .904.397.904.884s-.406.884-.904.884H5.946c-.499 0-.904-.397-.904-.884s.405-.884.904-.884zm0-4.875c.498 0 .904.396.904.884 0 .487-.406.883-.904.883H5.946c-.499 0-.904-.396-.904-.883 0-.488.405-.884.904-.884z'
																		fill='#EB0203' fill-rule='evenodd' />
																</svg>
															</span>
															<div class="available-note col-available-note d-none"></div>
														</a>
													</div>
												</div>
											</template>
											<div class="measurement-unit-section"
												v-if="isPriceAvailable && prodUnits ? prodUnits.hasAlternativeUnit : false">
												<div class="pwc-form-radio" @click="setDimension">
													<input type="radio" title
														class="pwc-form-check-input product-measurement-unit"
														:data-type="prodUnits.selectedUnit == prodUnits.secondaryUnit ? 'primary' : ''"
														:id="prodUnits.secondaryUnit + '-' + pid + '-measurement-unit-ptile' + additionalTag"
														:name="'measurement-unit-' + pid + '-ptile' + additionalTag"
														:value="prodUnits.secondaryUnit"
														v-model="prodUnits.selectedUnit" />
													<label
														:class="prodUnits.secondaryUnit + '-label pwc-form-check-label mb-0'"
														:for="prodUnits.secondaryUnit + '-' + pid + '-measurement-unit-ptile' + additionalTag">
														{{prodUnits.secondaryUnit}}
													</label>
													<span class="checkmark"></span>
												</div>
												<div class="pwc-form-radio" @click="setDimension">
													<input type="radio" title
														class="pwc-form-check-input product-measurement-unit"
														:data-type="prodUnits.selectedUnit == prodUnits.primaryUnit ? 'primary' : ''"
														:id="prodUnits.primaryUnit + '-' + pid + '-measurement-unit-ptile' + additionalTag"
														:name="'measurement-unit-' + pid + '-ptile' + additionalTag"
														:value="prodUnits.primaryUnit"
														v-model="prodUnits.selectedUnit" />
													<label
														:class="prodUnits.primaryUnit + '-label pwc-form-check-label mb-0'"
														:for="prodUnits.primaryUnit + '-' + pid + '-measurement-unit-ptile' + additionalTag">
														{{prodUnits.primaryUnit}}
													</label>
													<span class="checkmark"></span>
												</div>
											</div>
											<template v-if="!isPDP && isPriceAvailable">
												<span class="quantity-error pwc-span-error product-max-allocation"
													v-if="isMaxQtyReached">
													Quantidade m&aacute;xima atingida
												</span>
												<span
													class="quantity-error pwc-span-product col-span-product product-allocation"
													v-if="!isMinQtyReached && !isMaxQtyReached">
													{{quantitySelected}} no carrinho
												</span>
												<span class="quantity-error pwc-span-error product-min-allocation"
													:data-pid="pid" v-if="isMinQtyReached && !isMaxQtyReached">
													Quantidade m&iacute;nima atingida
												</span>
											</template>
										</template>
									</div>
									<input class="quantity-value-mask d-none" name="quantity" type="number" min="0"
										value />
								</div>
								<button type="button"
									class="add-to-cart button button--primary js-add-to-cart js-add-to-cart-tile col-button col-dynamic-layout-btn 2597619 button--primary col-button--primary dynamic-layout-btn"
									data-toggle="modal" data-target="#chooseBonusProductModal" data-pid="2597619"
									data-quantity-value="null" aria-label="Carrinho">
									<span class="d-block d-sm-none">
										<span class="svg-wrapper svg-icon-cart" style="width:20px; height:20px">
											<svg class='svg-inline icon-cart' xmlns='http://www.w3.org/2000/svg'
												viewBox='0 0 500 500' width='100%' height='100%'>
												<path fill='#EB0203' fill-rule='evenodd'
													d='M 485.67317,94.386311 C 476.37798,83.847129 463.09468,78.03348 448.30404,78.03348 H 90.690813 L 87.079511,51.327997 C 83.185582,22.539551 62.962285,0 41.011841,0 H 14.727836 C 6.563151,0 0,6.5286975 0,14.580758 c 0,8.05206 6.5945535,14.580758 14.727836,14.580758 h 26.284005 c 4.710398,0 14.696435,9.855222 16.894619,26.021523 L 100.01739,368.0631 c 1.88416,13.77244 9.01256,26.70549 20.1605,36.37417 8.25889,7.15046 18.02511,11.9071 28.26237,13.89679 -4.17655,7.58572 -6.53175,16.25957 -6.53175,25.493 0,29.53459 24.27424,53.59751 54.13815,53.59751 29.8325,0 54.13814,-24.03184 54.13814,-53.59751 0,-8.79818 -2.19818,-17.09897 -5.9979,-24.43598 h 77.12488 c -3.79973,7.33701 -5.96652,15.6378 -5.96652,24.43598 0,29.53459 24.27424,53.59751 54.13817,53.59751 29.83249,0 54.13814,-24.03184 54.13814,-53.59751 0,-8.79818 -2.1982,-17.09897 -5.9979,-24.43598 h 38.56243 c 8.13328,0 14.72783,-6.5287 14.72783,-14.61183 0,-8.02098 -6.59455,-14.54967 -14.72783,-14.54967 H 159.24277 c -14.38241,0 -28.13677,-11.90711 -30.02092,-26.02153 l -3.07746,-22.88154 h 290.6314 c 14.03698,0 27.94834,-5.28513 39.06488,-14.95381 11.11652,-9.6376 18.27632,-22.57065 20.16048,-36.37417 l 21.10259,-156.78201 c 1.94694,-14.51858 -2.10398,-28.29103 -11.43057,-38.861299 z M 220.76053,443.82706 c 0,13.49265 -11.08513,24.46709 -24.68247,24.46709 -13.62874,0 -24.71387,-10.97444 -24.71387,-24.46709 0,-13.49262 11.08513,-24.43598 24.71387,-24.43598 13.59734,0 24.68247,10.94336 24.68247,24.43598 z m 173.43675,0 c 0,13.49265 -11.08512,24.46709 -24.68245,24.46709 -13.62876,0 -24.71388,-10.97444 -24.71388,-24.46709 0,-13.49262 11.08512,-24.43598 24.71388,-24.43598 13.59733,0 24.68245,10.94336 24.68245,24.43598 z m 73.7648,-314.46558 -21.13397,156.782 c -1.88416,14.11442 -15.63851,26.05261 -30.02091,26.05261 H 122.25046 L 94.616142,107.19499 H 448.33544 c 6.18632,0 11.61896,2.2695 15.23028,6.37326 3.64269,4.13484 5.21283,9.73086 4.39636,15.79323 z' />
											</svg>
										</span>
									</span>
									<span class="add-to-cart--text d-none d-sm-block">Carrinho</span>
								</button>
							</div>
						</div>
					</div>
				</div>
				<!-- END_dwmarker -->
				<div class="col-12 d-sm-none">
				</div>
			</div>
		</div>
	</div>
</div>"""
