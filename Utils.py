import requests

class Pipe:
    def __init__(self, value):
        self.value = value
    
    def apply(self, func):
        return Pipe(func(self.value))
    
    def __repr__(self):
        return repr(self.value)

def pipe(value):
    return Pipe(value)

def make_request(url): 
    return requests.get(url)