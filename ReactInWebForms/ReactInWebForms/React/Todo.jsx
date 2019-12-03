import React from 'react';
import ReactDOM from 'react-dom';
import { hot } from 'react-hot-loader';

import TodoList from './TodoList';

class TodoApp extends React.Component {
    constructor(props) {
        super(props);
        this.state = { items: [], text: '' };
        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(e) {
        this.setState({ text: e.target.value });
    }

    handleSubmit(e) {
        e.preventDefault();
        if (!this.state.text.length) {
            return;
        }
        const newItem = {
            text: this.state.text,
            id: Date.now()
        };
        this.setState(state => ({
            items: state.items.concat(newItem),
            text: ''
        }));
    }

    render() {
        return (
            <div>
                <h3>Tarefas</h3>
                <TodoList items={this.state.items} />
                <form onSubmit={this.handleSubmit}>
                    <label htmlFor="new-todo">O que precisa ser feito?</label>
                    <input id="new-todo" onChange={this.handleChange} value={this.state.text} />
                    <button>
                        Adicionar #{this.state.items.length + 1}
                    </button>
                </form>
            </div>
        );
    }
}

const HotApp = hot(module)(TodoApp);
ReactDOM.render(<HotApp />, document.getElementById('divReactContainer'));