import json
from ContinenteScrapper import ContinenteScrapper
from PingoDoceScraper import PingoDoceScraper


def save_to_dynamodb(table):
    def save_item(item):
        table.put_item(Item=item)

    return save_item


def save_to_file(filename):
    def save_item(item):
        with open(filename, 'a') as file:
            json.dump(item, file)
            file.write('\n')

    return save_item


def scrape_website(name):

    if name == 'Continente':
        return ContinenteScrapper().scrape()
    elif name == 'Pingo Doce':
        return PingoDoceScraper().scrape()


# Define the configurations for each website
websites = [
    {
        'name': 'Continente'
    },
    {
        'name': 'Pingo Doce'
    },
    # {
    #     'name': 'Lidl',
    #     'url': 'https://www.lidl.pt',
    #     'product_selector': 'div.product__wrap',
    #     'name_selector': 'span.product__title',
    #     'price_selector': 'span.pricebox__price'
    # },
    # {
    #     'name': 'Aldi',
    #     'url': 'https://www.aldi.pt',
    #     'product_selector': 'div.product',
    #     'name_selector': 'span.product__title',
    #     'price_selector': 'span.product__price-value'
    # }
]

# Main program
# dynamodb = boto3.resource('dynamodb', region_name='YOUR_REGION', aws_access_key_id='YOUR_ACCESS_KEY', aws_secret_access_key='YOUR_SECRET_ACCESS_KEY')
# table = dynamodb.Table('YOUR_TABLE_NAME')

# Choose either DynamoDB or File saving method
# save_method = save_to_dynamodb(table)
save_method = save_to_file('output.txt')

for website in websites:
    print(f'Scraping {website["name"]}:')
    items = scrape_website(website['name'])
    save_item = save_method
    for item in items:
        save_item(item)
