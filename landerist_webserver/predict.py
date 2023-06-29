from flask import Flask, request
import pickle
import numpy as np

# Create an instance of the Flask class that is the WSGI application.
# The first argument is the name of the application module or package,
# typically __name__ when using a single module.
app = Flask(__name__)

with open('E:\Landerist\MLModel\Danyalktk\IsListing.pkl', 'rb') as f:
    model = pickle.load(f)

# Flask route decorators map / and /hello to the hello function.
# To add other resources, create functions that generate the page contents
# and add decorators to define the appropriate resource locators for them.


@app.route('/predict', methods=['POST'])
#@app.route('/predict')
def predict():
    data = request.json['input']    
    data = 11679
    data = np.array(data).reshape(1, -1)    
    prediction = model.predict(data)
    return str(prediction[0]);    


@app.route('/')
@app.route('/', methods=['POST'])
def hello():
    # Render the page
    return "Landerist Predictor!"
    #data = 11679
    #data = np.array(data).reshape(1, -1)    
    #prediction = model.predict(data)
    #return prediction.tolist();

if __name__ == '__main__':
    #app.run(port=5000)
    app.run('localhost', 4449)