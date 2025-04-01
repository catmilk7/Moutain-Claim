from flask import Blueprint, render_template, request, flash, redirect, url_for
from .models import User
from werkzeug.security import generate_password_hash, check_password_hash
from . import db


auth = Blueprint('auth', __name__)

@auth.route('/login', methods=['GET', 'POST'])
def login():
    if request.method == 'POST':
        email = request.form.get('email')
        password = request.form.get('password')

        user = User.query.filter_by(email=email).first()
        if user:
            if check_password_hash(user.password, password):
                flash('Logged in successfully!', category='success')
                return redirect(url_for('views.home'))
            else:
                flash('Incorrect password', category='error')
        else:
            flash('Email does not exist.', category='error')

    return render_template("login.html")

@auth.route('/register', methods=['GET', 'POST'])
def register():
    if request.method == 'POST':
        full_name = request.form.get('full_name')
        email = request.form.get('email')
        password = request.form.get('password')
        confirm_password = request.form.get('confirm_password')


        user = User.query.filter_by(email=email).first()

        if user:
            flash('Email already exists.', category='error')
        elif len(email) < 4:
            flash('Email must be greater than 3 characters.', category='error')
        elif len(full_name) < 2:
            flash('Full name must be greater than 1 character.', category='error')
        elif password != confirm_password:
            flash('Passwords don\'t match', category='error')
        elif len(password) < 7:
            flash('Password must be greater than 6 characters.', category='error')
        else:
            new_user = User(email=email, full_name=full_name, password=generate_password_hash(password, method='pbkdf2:sha256'))
            db.session.add(new_user)
            db.session.commit()
            flash('Account created', category='success')
            return redirect(url_for('views.home'))
        
    return render_template("register.html")

@auth.route('/dashboard')
def dashboard():
    return render_template("dashboard.html")