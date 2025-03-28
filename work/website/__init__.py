from flask import Flask
from flask_sqlalchemy import SQLAlchemy
from os import path
import os

db = SQLAlchemy()
DB_NAME = "database.db"
DB_FOLDER = "website"
DB_PATH = os.path.join(os.getcwd(), DB_FOLDER, DB_NAME)  # Get absolute path

def create_app():
    app = Flask(__name__)
    app.config['SECRET_KEY'] = 'abcdefg'
    app.config['SQLALCHEMY_DATABASE_URI'] = f'sqlite:///{DB_PATH}'  # Ensure absolute path
    db.init_app(app)

    from .views import views
    from .auth import auth

    app.register_blueprint(views, url_prefix='/')
    app.register_blueprint(auth, url_prefix='/')

    from .models import User, Note

    create_database(app)

    return app

def create_database(app):
    if not os.path.exists(DB_FOLDER):  # Ensure folder exists
        os.makedirs(DB_FOLDER, exist_ok=True)  # Create directory safely
    if not path.exists('website/' + DB_NAME):
        with app.app_context():
            db.create_all()
            print('Database Created at:', DB_PATH)

