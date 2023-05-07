from datetime import date
import json
from Utils import make_request, pipe


class PingoDoceScraper():
    def __init__(self):
        self.name = 'Pingo Doce'
        self.url = 'https://mercadao.pt/api/catalogues/6107d28d72939a003ff6bf51/products/search?query=[]&from=0&size=10&esPreference=0.6774024649118144'

    def scrape(self):
        products = pipe(self.url).apply(make_request).apply(self.parse_response).apply(self.extract_products).value
        # Perform the necessary scraping logic for Pingo Doce
        return products

    def parse_response(self, response):
        data = json.loads(response.text)
        return data

    def extract_products(self, data):
        products = []
        for product in data['sections']['null']['products']:
            product_data = product['_source']
            product_info = {
                'Name': product_data['firstName'],
                'Price': product_data['unitPrice'],
                'PriceUnit': product_data['netContentUnit'].lower(),
                'Source': self.name,
                'Date': str(date.today())
            }
            products.append(product_info)
        return products