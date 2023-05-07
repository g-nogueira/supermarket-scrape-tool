from datetime import date
import re
import string
from bs4 import BeautifulSoup, Tag
from Utils import make_request, pipe

class ContinenteScrapper():
    def __init__(self):
        self.url = "https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Search-UpdateGrid?cgid=col-produtos&pmin=0%2e01&start=0&sz=24"
        self.name = 'Continente'
        self.product_selector = '[data-pid].product'
        self.name_selector = '[data-pid].product .pwc-tile--description'
        self.price_selector = '[data-pid].product .js-product-price .value'
        self.price_selector_attr = 'content'
        self.price_unit_selector = '[data-pid].product .pwc-m-unit'
    
    def scrape(self):
        return pipe(self.url).apply(make_request).apply(self.extract_products).value

    def extract_products(self, response):
        soup = BeautifulSoup(response.text, 'html.parser')
        products = soup.select(self.product_selector)

        def get_price(htmlelement: Tag):
            if self.price_selector_attr:
                return htmlelement.attrs.get(self.price_selector_attr).strip().replace(',', '.')
            else:
                return htmlelement.text.strip().replace(',', '.')
        
        def get_price_element(product: Tag):
            return product.select_one(self.price_selector)
        
        def get_price_unit(product: Tag) -> string:
            text = product.select_one(self.price_unit_selector).text

            return re.findall(r"([a-zA-Z]+)",text)[0]

        return [{
            'Name': product.select_one(self.name_selector).text.strip(),
            'Price': float(pipe(product).apply(get_price_element).apply(get_price).value),
            'PriceUnit': get_price_unit(product).lower(),
            'Source': self.name,
            'Date': str(date.today())
        } for product in products]