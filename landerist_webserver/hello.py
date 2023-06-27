from flask import Flask

# Create an instance of the Flask class that is the WSGI application.
# The first argument is the name of the application module or package,
# typically __name__ when using a single module.
app = Flask(__name__)

# Flask route decorators map / and /hello to the hello function.
# To add other resources, create functions that generate the page contents
# and add decorators to define the appropriate resource locators for them.

@app.route('/')
@app.route('/hello')
def hello():
    # Render the page
    return "Hello Python!"


@app.route('/predict')
def predict():
    #data = request.json
    #prediction = modelo.predict(np.array(data['input']).reshape(1, -1))
    #return {'prediction': prediction.tolist()}
    return "predict"

if __name__ == '__main__':
    # Run the app server on localhost:4449
    app.run('localhost', 4449)
